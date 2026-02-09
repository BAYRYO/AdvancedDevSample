using System.Data.Common;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Domain.ValueObjects;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Entities;
using AdvancedDevSample.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Test.Infrastructure.Persistence;

public class EfRepositoryAndTransactionTests
{
    [Fact]
    public async Task EfCategoryRepository_Should_Save_Query_And_Delete()
    {
        using SqliteHarness harness = CreateHarness();
        EfCategoryRepository repository = new EfCategoryRepository(harness.Context);

        Category active = new Category("Electronics", "Devices");
        Category inactive = new Category("Archived", "Old");
        inactive.Deactivate();

        await repository.SaveAsync(active);
        await repository.SaveAsync(inactive);

        IReadOnlyList<Category> all = await repository.GetAllAsync();
        IReadOnlyList<Category> onlyActive = await repository.GetActiveAsync();

        Assert.Equal(2, all.Count);
        Assert.Single(onlyActive);
        Assert.Equal(active.Id, onlyActive[0].Id);
        Assert.True(await repository.ExistsAsync(active.Id));

        await repository.DeleteAsync(active.Id);
        await repository.DeleteAsync(Guid.NewGuid());

        Assert.False(await repository.ExistsAsync(active.Id));
    }

    [Fact]
    public async Task EfProductRepository_Should_Search_Save_Update_And_Delete()
    {
        using SqliteHarness harness = CreateHarness();
        EfCategoryRepository categoryRepository = new EfCategoryRepository(harness.Context);
        EfProductRepository repository = new EfProductRepository(harness.Context);

        Category category = new Category("Phones", "Mobile");
        await categoryRepository.SaveAsync(category);

        Product product = new Product("Phone X", 499m, new Sku("phx-001"), 20, categoryId: category.Id);
        await repository.SaveAsync(product);

        Product? bySku = await repository.GetBySkuAsync("phx-001");
        Assert.NotNull(bySku);
        Assert.Equal(product.Id, bySku.Id);

        ProductSearchCriteria criteria = new ProductSearchCriteria(
            Name: "Phone",
            MinPrice: 100m,
            MaxPrice: 800m,
            CategoryId: category.Id,
            IsActive: true,
            Page: 1,
            PageSize: 10);

        (IReadOnlyList<Product> items, int totalCount) = await repository.SearchAsync(criteria);
        Assert.Single(items);
        Assert.Equal(1, totalCount);
        Assert.True(await repository.ExistsWithSkuAsync("PHX-001"));
        Assert.False(await repository.ExistsWithSkuAsync("PHX-001", product.Id));

        product.UpdateName("Phone X2");
        repository.Save(product);

        Product? updated = repository.GetById(product.Id);
        Assert.NotNull(updated);
        Assert.Equal("Phone X2", updated.Name);

        await repository.DeleteAsync(product.Id);
        await repository.DeleteAsync(Guid.NewGuid());
        Assert.Null(await repository.GetByIdAsync(product.Id));
    }

    [Fact]
    public async Task EfUserRepository_Should_Normalize_Email_And_Update_Existing_User()
    {
        using SqliteHarness harness = CreateHarness();
        EfUserRepository repository = new EfUserRepository(harness.Context);

        User first = new User("Alpha@example.com", "hash-1", "Alpha", "User");
        User second = new User("beta@example.com", "hash-2", "Beta", "User");

        await repository.SaveAsync(first);
        await repository.SaveAsync(second);

        User? byEmail = await repository.GetByEmailAsync("  ALPHA@EXAMPLE.COM ");
        Assert.NotNull(byEmail);
        Assert.Equal(first.Id, byEmail.Id);
        Assert.True(await repository.ExistsByEmailAsync("alpha@example.com"));

        first.ChangeRole(UserRole.Admin);
        first.UpdateName("Updated", "Admin");
        await repository.SaveAsync(first);

        User? updated = await repository.GetByIdAsync(first.Id);
        Assert.NotNull(updated);
        Assert.Equal(UserRole.Admin, updated.Role);
        Assert.Equal("Updated", updated.FirstName);

        IEnumerable<User> page = await repository.GetAllAsync(page: 1, pageSize: 1);
        Assert.Single(page);
        Assert.Equal(2, await repository.GetCountAsync());
    }

    [Fact]
    public async Task EfRefreshTokenRepository_Should_Save_Query_And_Revoke_Tokens()
    {
        using SqliteHarness harness = CreateHarness();
        EfUserRepository userRepository = new EfUserRepository(harness.Context);
        EfRefreshTokenRepository tokenRepository = new EfRefreshTokenRepository(harness.Context);

        User user = new User("token-owner@example.com", "hash", "Token", "Owner");
        await userRepository.SaveAsync(user);

        RefreshToken token = new RefreshToken(user.Id, expirationDays: 3);
        await tokenRepository.SaveAsync(token);

        RefreshTokenEntity persistedToken = await harness.Context.RefreshTokens.SingleAsync(t => t.Id == token.Id);
        Assert.NotEqual(token.GetPlainTextTokenOrThrow(), persistedToken.Token);

        RefreshToken? found = await tokenRepository.GetByTokenAsync(token.GetPlainTextTokenOrThrow());
        Assert.NotNull(found);
        Assert.Equal(token.Id, found.Id);

        IEnumerable<RefreshToken> byUser = await tokenRepository.GetByUserIdAsync(user.Id);
        Assert.Single(byUser);

        token.Revoke();
        await tokenRepository.SaveAsync(token);
        await tokenRepository.RevokeAllForUserAsync(user.Id);

        RefreshToken? reloaded = await tokenRepository.GetByTokenAsync(token.GetPlainTextTokenOrThrow());
        Assert.NotNull(reloaded);
        Assert.True(reloaded.IsRevoked);
    }

    [Fact]
    public async Task EfAuditLogRepository_Should_Filter_Limit_And_Order_Logs()
    {
        using SqliteHarness harness = CreateHarness();
        EfAuditLogRepository repository = new EfAuditLogRepository(harness.Context);

        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();

        AuditLog oldest = new AuditLog(new AuditLog.ReconstitutionData
        {
            Id = Guid.NewGuid(),
            EventType = AuditLog.EventTypes.LoginSuccess,
            UserId = userA,
            UserEmail = "a@x.com",
            IpAddress = null,
            UserAgent = null,
            IsSuccess = true,
            Details = "old",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        });
        AuditLog middle = new AuditLog(new AuditLog.ReconstitutionData
        {
            Id = Guid.NewGuid(),
            EventType = AuditLog.EventTypes.LoginFailure,
            UserId = userA,
            UserEmail = "a@x.com",
            IpAddress = null,
            UserAgent = null,
            IsSuccess = false,
            Details = "mid",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        AuditLog newest = new AuditLog(new AuditLog.ReconstitutionData
        {
            Id = Guid.NewGuid(),
            EventType = AuditLog.EventTypes.Register,
            UserId = userB,
            UserEmail = "b@x.com",
            IpAddress = null,
            UserAgent = null,
            IsSuccess = true,
            Details = "new",
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        });

        await repository.SaveAsync(oldest);
        await repository.SaveAsync(middle);
        await repository.SaveAsync(newest);

        IEnumerable<AuditLog> userARecent = await repository.GetByUserIdAsync(userA, limit: 1);
        AuditLog latestForUserA = Assert.Single(userARecent);
        Assert.Equal(middle.Id, latestForUserA.Id);

        List<AuditLog> globalRecent = (await repository.GetRecentAsync(limit: 2)).ToList();
        Assert.Equal(2, globalRecent.Count);
        Assert.Equal(newest.Id, globalRecent[0].Id);
        Assert.Equal(middle.Id, globalRecent[1].Id);
    }

    [Fact]
    public async Task EfPriceHistoryRepository_Should_Save_And_Order_By_ChangedAt_Descending()
    {
        using SqliteHarness harness = CreateHarness();
        EfProductRepository productRepository = new EfProductRepository(harness.Context);
        EfPriceHistoryRepository historyRepository = new EfPriceHistoryRepository(harness.Context);

        Product product = new Product("Tracked Product", 100m, new Sku("TRACK-001"));
        await productRepository.SaveAsync(product);

        PriceHistory older = new PriceHistory(Guid.NewGuid(), product.Id, 80m, 90m, null, DateTime.UtcNow.AddDays(-2), "Older");
        PriceHistory newer = new PriceHistory(Guid.NewGuid(), product.Id, 90m, 100m, 5m, DateTime.UtcNow.AddDays(-1), "Newer");

        await historyRepository.SaveAsync(older);
        await historyRepository.SaveAsync(newer);

        IReadOnlyList<PriceHistory> history = await historyRepository.GetByProductIdAsync(product.Id);
        Assert.Equal(2, history.Count);
        Assert.Equal(newer.Id, history[0].Id);
        Assert.Equal(older.Id, history[1].Id);
    }

    [Fact]
    public async Task EfTransactionManager_Should_Commit_And_Support_Existing_Transaction()
    {
        using SqliteHarness harness = CreateHarness();
        EfTransactionManager manager = new EfTransactionManager(harness.Context);
        Guid categoryId = Guid.NewGuid();

        await manager.ExecuteInTransactionAsync(async () =>
        {
            harness.Context.Categories.Add(new CategoryEntity
            {
                Id = categoryId,
                Name = "Committed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await harness.Context.SaveChangesAsync();
        });

        CategoryEntity? persisted = await harness.Context.Categories.FindAsync(categoryId);
        Assert.NotNull(persisted);

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction outer = await harness.Context.Database.BeginTransactionAsync();
        bool branchExecuted = false;

        await manager.ExecuteInTransactionAsync(async () =>
        {
            branchExecuted = true;
            await Task.CompletedTask;
        });

        Assert.True(branchExecuted);
        await outer.RollbackAsync();
    }

    [Fact]
    public async Task EfTransactionManager_Generic_Should_Rollback_When_Exception_Is_Thrown()
    {
        using SqliteHarness harness = CreateHarness();
        EfTransactionManager manager = new EfTransactionManager(harness.Context);
        Guid categoryId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await manager.ExecuteInTransactionAsync<int>(async () =>
            {
                harness.Context.Categories.Add(new CategoryEntity
                {
                    Id = categoryId,
                    Name = "Rollback",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await harness.Context.SaveChangesAsync();
                throw new InvalidOperationException("boom");
            });
        });

        harness.Context.ChangeTracker.Clear();
        CategoryEntity? category = await harness.Context.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == categoryId);
        Assert.Null(category);
    }

    private static SqliteHarness CreateHarness()
    {
        SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        AppDbContext context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return new SqliteHarness(context, connection);
    }

    private sealed class SqliteHarness : IDisposable
    {
        public AppDbContext Context { get; }
        private readonly DbConnection _connection;

        public SqliteHarness(AppDbContext context, DbConnection connection)
        {
            Context = context;
            _connection = connection;
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
