using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AdvancedDevSample.Infrastructure.Persistence.Seeders;

public class DatabaseSeeder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IServiceProvider serviceProvider, ILogger<DatabaseSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        ISeeder[] seeders = [.. GetSeeders().OrderBy(s => s.Order)];

        _logger.LogInformation("Starting database seeding with {SeederCount} seeders", seeders.Length);

        foreach (ISeeder seeder in seeders)
        {
            string seederName = seeder.GetType().Name;
            _logger.LogInformation("Running seeder: {SeederName}", seederName);

            try
            {
                await seeder.SeedAsync(context, cancellationToken);
                _logger.LogInformation("Completed seeder: {SeederName}", seederName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running seeder: {SeederName}", seederName);
                throw new InvalidOperationException($"Error running seeder: {seederName}", ex);
            }
        }

        _logger.LogInformation("Database seeding completed");
    }

    private static ISeeder[] GetSeeders()
    {
        return
        [
            new AdminUserSeeder(),
            new CategorySeeder(),
            new ProductSeeder(),
            new PriceHistorySeeder()
        ];
    }
}

public static class DatabaseSeederExtensions
{
    public static IServiceCollection AddDatabaseSeeder(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        return services;
    }

    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        DatabaseSeeder seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
