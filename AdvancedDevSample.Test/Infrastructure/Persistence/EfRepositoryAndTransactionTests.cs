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
        using var harness = CreateHarness();
        var repository = new EfCategoryRepository(harness.Context);

        var active = new Category("Electronics", "Devices");
        var inactive = new Category("Archived", "Old");
        inactive.Deactivate();

        await repository.SaveAsync(active);
        await repository.SaveAsync(inactive);

        var all = await repository.GetAllAsync();
        var onlyActive = await repository.GetActiveAsync();

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
        using var harness = CreateHarness();
        var categoryRepository = new EfCategoryRepository(harness.Context);
        var repository = new EfProductRepository(harness.Context);

        var category = new Category("Phones", "Mobile");
        await categoryRepository.SaveAsync(category);

        var product = new Product("Phone X", 499m, new Sku("phx-001"), 20, categoryId: category.Id);
        await repository.SaveAsync(product);

        var bySku = await repository.GetBySkuAsync("phx-001");
        Assert.NotNull(bySku);
        Assert.Equal(product.Id, bySku.Id);

        var criteria = new ProductSearchCriteria(
            Name: "Phone",
            MinPrice: 100m,
            MaxPrice: 800m,
            CategoryId: category.Id,
            IsActive: true,
            Page: 1,
            PageSize: 10);

        var (items, totalCount) = await repository.SearchAsync(criteria);
        Assert.Single(items);
        Assert.Equal(1, totalCount);
        Assert.True(await repository.ExistsWithSkuAsync("PHX-001"));
        Assert.False(await repository.ExistsWithSkuAsync("PHX-001", product.Id));

        product.UpdateName("Phone X2");
        repository.Save(product);

        var updated = repository.GetById(product.Id);
        Assert.NotNull(updated);
        Assert.Equal("Phone X2", updated.Name);

        await repository.DeleteAsync(product.Id);
        await repository.DeleteAsync(Guid.NewGuid());
        Assert.Null(await repository.GetByIdAsync(product.Id));
    }

    [Fact]
    public async Task EfUserRepository_Should_Normalize_Email_And_Update_Existing_User()
    {
        using var harness = CreateHarness();
        var repository = new EfUserRepository(harness.Context);

        var first = new User("Alpha@example.com", "hash-1", "Alpha", "User");
        var second = new User("beta@example.com", "hash-2", "Beta", "User");

        await repository.SaveAsync(first);
        await repository.SaveAsync(second);

        var byEmail = await repository.GetByEmailAsync("  ALPHA@EXAMPLE.COM ");
        Assert.NotNull(byEmail);
        Assert.Equal(first.Id, byEmail.Id);
        Assert.True(await repository.ExistsByEmailAsync("alpha@example.com"));

        first.ChangeRole(UserRole.Admin);
        first.UpdateName("Updated", "Admin");
        await repository.SaveAsync(first);

        var updated = await repository.GetByIdAsync(first.Id);
        Assert.NotNull(updated);
        Assert.Equal(UserRole.Admin, updated.Role);
        Assert.Equal("Updated", updated.FirstName);

        var page = await repository.GetAllAsync(page: 1, pageSize: 1);
        Assert.Single(page);
        Assert.Equal(2, await repository.GetCountAsync());
    }

    [Fact]
    public async Task EfRefreshTokenRepository_Should_Save_Query_And_Revoke_Tokens()
    {
        using var harness = CreateHarness();
        var userRepository = new EfUserRepository(harness.Context);
        var tokenRepository = new EfRefreshTokenRepository(harness.Context);

        var user = new User("token-owner@example.com", "hash", "Token", "Owner");
        await userRepository.SaveAsync(user);

        var token = new RefreshToken(user.Id, expirationDays: 3);
        await tokenRepository.SaveAsync(token);

        RefreshTokenEntity persistedToken = await harness.Context.RefreshTokens.SingleAsync(t => t.Id == token.Id);
        Assert.NotEqual(token.GetPlainTextTokenOrThrow(), persistedToken.Token);

        RefreshToken? found = await tokenRepository.GetByTokenAsync(token.GetPlainTextTokenOrThrow());
        Assert.NotNull(found);
        Assert.Equal(token.Id, found.Id);

        var byUser = await tokenRepository.GetByUserIdAsync(user.Id);
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
        using var harness = CreateHarness();
        var repository = new EfAuditLogRepository(harness.Context);

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var oldest = new AuditLog(new AuditLog.ReconstitutionData
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
        var middle = new AuditLog(new AuditLog.ReconstitutionData
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
        var newest = new AuditLog(new AuditLog.ReconstitutionData
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

        var userARecent = await repository.GetByUserIdAsync(userA, limit: 1);
        var latestForUserA = Assert.Single(userARecent);
        Assert.Equal(middle.Id, latestForUserA.Id);

        var globalRecent = (await repository.GetRecentAsync(limit: 2)).ToList();
        Assert.Equal(2, globalRecent.Count);
        Assert.Equal(newest.Id, globalRecent[0].Id);
        Assert.Equal(middle.Id, globalRecent[1].Id);
    }

    [Fact]
    public async Task EfPriceHistoryRepository_Should_Save_And_Order_By_ChangedAt_Descending()
    {
        using var harness = CreateHarness();
        var productRepository = new EfProductRepository(harness.Context);
        var historyRepository = new EfPriceHistoryRepository(harness.Context);

        var product = new Product("Tracked Product", 100m, new Sku("TRACK-001"));
        await productRepository.SaveAsync(product);

        var older = new PriceHistory(Guid.NewGuid(), product.Id, 80m, 90m, null, DateTime.UtcNow.AddDays(-2), "Older");
        var newer = new PriceHistory(Guid.NewGuid(), product.Id, 90m, 100m, 5m, DateTime.UtcNow.AddDays(-1), "Newer");

        await historyRepository.SaveAsync(older);
        await historyRepository.SaveAsync(newer);

        var history = await historyRepository.GetByProductIdAsync(product.Id);
        Assert.Equal(2, history.Count);
        Assert.Equal(newer.Id, history[0].Id);
        Assert.Equal(older.Id, history[1].Id);
    }

    [Fact]
    public async Task EfTransactionManager_Should_Commit_And_Support_Existing_Transaction()
    {
        using var harness = CreateHarness();
        var manager = new EfTransactionManager(harness.Context);
        var categoryId = Guid.NewGuid();

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

        var persisted = await harness.Context.Categories.FindAsync(categoryId);
        Assert.NotNull(persisted);

        await using var outer = await harness.Context.Database.BeginTransactionAsync();
        var branchExecuted = false;

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
        using var harness = CreateHarness();
        var manager = new EfTransactionManager(harness.Context);
        var categoryId = Guid.NewGuid();

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
        var category = await harness.Context.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == categoryId);
        Assert.Null(category);
    }

    private static SqliteHarness CreateHarness()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
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
