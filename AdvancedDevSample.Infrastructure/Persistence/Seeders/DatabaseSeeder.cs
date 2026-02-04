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
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seeders = GetSeeders().OrderBy(s => s.Order).ToList();

        _logger.LogInformation("Starting database seeding with {SeederCount} seeders", seeders.Count);

        foreach (var seeder in seeders)
        {
            var seederName = seeder.GetType().Name;
            _logger.LogInformation("Running seeder: {SeederName}", seederName);

            try
            {
                await seeder.SeedAsync(context, cancellationToken);
                _logger.LogInformation("Completed seeder: {SeederName}", seederName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running seeder: {SeederName}", seederName);
                throw;
            }
        }

        _logger.LogInformation("Database seeding completed");
    }

    private static IEnumerable<ISeeder> GetSeeders()
    {
        return new ISeeder[]
        {
            new CategorySeeder(),
            new ProductSeeder(),
            new PriceHistorySeeder()
        };
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
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
