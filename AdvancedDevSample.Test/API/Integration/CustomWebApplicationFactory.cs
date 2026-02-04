using AdvancedDevSample.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AdvancedDevSample.Test.API.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryProductRepository ProductRepository { get; } = new();
    public InMemoryCategoryRepository CategoryRepository { get; } = new();
    public InMemoryPriceHistoryRepository PriceHistoryRepository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedDatabase"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IProductRepository));
            services.RemoveAll(typeof(ICategoryRepository));
            services.RemoveAll(typeof(IPriceHistoryRepository));

            services.AddSingleton<IProductRepository>(ProductRepository);
            services.AddSingleton<ICategoryRepository>(CategoryRepository);
            services.AddSingleton<IPriceHistoryRepository>(PriceHistoryRepository);
        });
    }
}
