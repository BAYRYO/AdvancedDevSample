using System.Data.Common;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Entities;
using AdvancedDevSample.Infrastructure.Persistence.Seeders;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedDevSample.Test.Infrastructure.Persistence;

public class SeedingAndFactoryTests
{
    [Fact]
    public async Task DatabaseSeeder_Should_Seed_All_Data_And_Be_Idempotent()
    {
        string? previousEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        string? previousPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "admin.seed@example.com");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "StrongPassword!123");

        try
        {
            await using ServiceProvider provider = BuildSeederServiceProvider();

            await provider.SeedDatabaseAsync();

            using (IServiceScope scope = provider.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Assert.True(await context.Users.AnyAsync());
                Assert.True(await context.Categories.CountAsync() >= 5);
                Assert.True(await context.Products.CountAsync() >= 5);
                Assert.True(await context.PriceHistories.AnyAsync());
            }

            int usersAfterFirstSeed;
            int categoriesAfterFirstSeed;
            int productsAfterFirstSeed;
            int historyAfterFirstSeed;

            using (IServiceScope scope = provider.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                usersAfterFirstSeed = await context.Users.CountAsync();
                categoriesAfterFirstSeed = await context.Categories.CountAsync();
                productsAfterFirstSeed = await context.Products.CountAsync();
                historyAfterFirstSeed = await context.PriceHistories.CountAsync();
            }

            await provider.SeedDatabaseAsync();

            using (IServiceScope scope = provider.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Assert.Equal(usersAfterFirstSeed, await context.Users.CountAsync());
                Assert.Equal(categoriesAfterFirstSeed, await context.Categories.CountAsync());
                Assert.Equal(productsAfterFirstSeed, await context.Products.CountAsync());
                Assert.Equal(historyAfterFirstSeed, await context.PriceHistories.CountAsync());
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("ADMIN_EMAIL", previousEmail);
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", previousPassword);
        }
    }

    [Fact]
    public async Task DatabaseSeeder_Should_Continue_When_Admin_Environment_Is_Missing()
    {
        string? previousEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        string? previousPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);

        try
        {
            await using ServiceProvider provider = BuildSeederServiceProvider();
            await provider.SeedDatabaseAsync();

            using IServiceScope scope = provider.CreateScope();
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Equal(0, await context.Users.CountAsync(u => u.Role == (int)UserRole.Admin));
            Assert.True(await context.Categories.CountAsync() >= 5);
            Assert.True(await context.Products.CountAsync() >= 5);
            Assert.True(await context.PriceHistories.AnyAsync());
        }
        finally
        {
            Environment.SetEnvironmentVariable("ADMIN_EMAIL", previousEmail);
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", previousPassword);
        }
    }

    [Fact]
    public async Task AdminUserSeeder_Should_Return_Immediately_When_Admin_Already_Exists()
    {
        using SqliteHarness harness = CreateHarness();
        AdminUserSeeder seeder = new AdminUserSeeder();

        harness.Context.Users.Add(new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "existing-admin@example.com",
            PasswordHash = "hash",
            FirstName = "Existing",
            LastName = "Admin",
            Role = (int)UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await harness.Context.SaveChangesAsync();

        await seeder.SeedAsync(harness.Context);

        Assert.Equal(1, await harness.Context.Users.CountAsync(u => u.Role == (int)UserRole.Admin));
    }

    [Fact]
    public void AppDbContextFactory_Should_Use_Default_And_Environment_Connection_String()
    {
        AppDbContextFactory factory = new AppDbContextFactory();
        string? previous = Environment.GetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING");

        try
        {
            Environment.SetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING", null);
            using AppDbContext defaultContext = factory.CreateDbContext([]);
            string? defaultConnection = defaultContext.Database.GetConnectionString();
            Assert.NotNull(defaultConnection);
            Assert.Contains("advanceddevsample.db", defaultConnection!, StringComparison.OrdinalIgnoreCase);

            Environment.SetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING", "Data Source=:memory:");
            using AppDbContext envContext = factory.CreateDbContext([]);
            Assert.Equal("Data Source=:memory:", envContext.Database.GetConnectionString());
        }
        finally
        {
            Environment.SetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING", previous);
        }
    }

    private static ServiceProvider BuildSeederServiceProvider()
    {
        ServiceCollection services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<DbConnection>(_ =>
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            return connection;
        });
        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseSqlite(sp.GetRequiredService<DbConnection>()));
        services.AddDatabaseSeeder();

        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope = provider.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        return provider;
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
