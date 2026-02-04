namespace AdvancedDevSample.Infrastructure.Persistence.Seeders;

public interface ISeeder
{
    int Order { get; }
    Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default);
}
