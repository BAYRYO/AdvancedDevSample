using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using AdvancedDevSample.Application.Interfaces;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace AdvancedDevSample.Test.API.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "test-jwt-secret-key-for-integration-tests-minimum-32-characters";
    private const string TestIssuer = "AdvancedDevSample";
    private const string TestAudience = "AdvancedDevSample";
    private readonly string _inMemoryDatabaseName = $"advanceddevsample-tests-{Guid.NewGuid():N}";

    public InMemoryProductRepository ProductRepository { get; } = new();
    public InMemoryCategoryRepository CategoryRepository { get; } = new();
    public InMemoryPriceHistoryRepository PriceHistoryRepository { get; } = new();
    public InMemoryUserRepository UserRepository { get; } = new();
    public InMemoryRefreshTokenRepository RefreshTokenRepository { get; } = new();
    public InMemoryAuditLogRepository AuditLogRepository { get; } = new();

    public CustomWebApplicationFactory()
    {
        // Program startup reads JWT secret from environment first.
        Environment.SetEnvironmentVariable("JWT_SECRET", TestJwtSecret);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedDatabase"] = "false",
                ["UseMigrations"] = "false",
                ["UseInMemoryDatabase"] = "true",
                ["InMemoryDatabaseName"] = _inMemoryDatabaseName
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_inMemoryDatabaseName));

            services.RemoveAll<IProductRepository>();
            services.RemoveAll<ICategoryRepository>();
            services.RemoveAll<IPriceHistoryRepository>();
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IRefreshTokenRepository>();
            services.RemoveAll<IAuditLogRepository>();

            services.AddSingleton<IProductRepository>(ProductRepository);
            services.AddSingleton<ICategoryRepository>(CategoryRepository);
            services.AddSingleton<IPriceHistoryRepository>(PriceHistoryRepository);
            services.AddSingleton<IUserRepository>(UserRepository);
            services.AddSingleton<IRefreshTokenRepository>(RefreshTokenRepository);
            services.AddSingleton<IAuditLogRepository>(AuditLogRepository);
        });
    }

    public HttpClient CreateAuthenticatedClient(UserRole role = UserRole.User)
    {
        HttpClient client = CreateClient();
        string token = GenerateTestToken(Guid.NewGuid(), "test@example.com", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static string GenerateTestToken(Guid userId, string email, UserRole role)
    {
        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
