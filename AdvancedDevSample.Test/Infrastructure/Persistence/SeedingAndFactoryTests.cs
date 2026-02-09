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
        var previousEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var previousPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "admin.seed@example.com");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "StrongPassword!123");

        try
        {
            await using var provider = BuildSeederServiceProvider();

            await provider.SeedDatabaseAsync();

            using (var scope = provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Assert.True(await context.Users.CountAsync() >= 1);
                Assert.True(await context.Categories.CountAsync() >= 5);
                Assert.True(await context.Products.CountAsync() >= 5);
                Assert.True(await context.PriceHistories.CountAsync() > 0);
            }

            int usersAfterFirstSeed;
            int categoriesAfterFirstSeed;
            int productsAfterFirstSeed;
            int historyAfterFirstSeed;

            using (var scope = provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                usersAfterFirstSeed = await context.Users.CountAsync();
                categoriesAfterFirstSeed = await context.Categories.CountAsync();
                productsAfterFirstSeed = await context.Products.CountAsync();
                historyAfterFirstSeed = await context.PriceHistories.CountAsync();
            }

            await provider.SeedDatabaseAsync();

            using (var scope = provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
        var previousEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var previousPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);

        try
        {
            await using var provider = BuildSeederServiceProvider();
            await provider.SeedDatabaseAsync();

            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Equal(0, await context.Users.CountAsync(u => u.Role == (int)UserRole.Admin));
            Assert.True(await context.Categories.CountAsync() >= 5);
            Assert.True(await context.Products.CountAsync() >= 5);
            Assert.True(await context.PriceHistories.CountAsync() > 0);
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
        using var harness = CreateHarness();
        var seeder = new AdminUserSeeder();

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
        var factory = new AppDbContextFactory();
        var previous = Environment.GetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING");

        try
        {
            Environment.SetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING", null);
            using var defaultContext = factory.CreateDbContext(Array.Empty<string>());
            var defaultConnection = defaultContext.Database.GetConnectionString();
            Assert.NotNull(defaultConnection);
            Assert.Contains("advanceddevsample.db", defaultConnection!, StringComparison.OrdinalIgnoreCase);

            Environment.SetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING", "Data Source=:memory:");
            using var envContext = factory.CreateDbContext(Array.Empty<string>());
            Assert.Equal("Data Source=:memory:", envContext.Database.GetConnectionString());
        }
        finally
        {
            Environment.SetEnvironmentVariable("DESIGNTIME_CONNECTION_STRING", previous);
        }
    }

    private static ServiceProvider BuildSeederServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<DbConnection>(_ =>
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            return connection;
        });
        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseSqlite(sp.GetRequiredService<DbConnection>()));
        services.AddDatabaseSeeder();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        return provider;
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
