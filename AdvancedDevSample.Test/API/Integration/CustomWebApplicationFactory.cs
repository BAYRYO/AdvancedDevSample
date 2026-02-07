using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using AdvancedDevSample.Application.Interfaces;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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

    public InMemoryProductRepository ProductRepository { get; } = new();
    public InMemoryCategoryRepository CategoryRepository { get; } = new();
    public InMemoryPriceHistoryRepository PriceHistoryRepository { get; } = new();
    public InMemoryUserRepository UserRepository { get; } = new();
    public InMemoryRefreshTokenRepository RefreshTokenRepository { get; } = new();
    public InMemoryAuditLogRepository AuditLogRepository { get; } = new();

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", TestJwtSecret);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedDatabase"] = "false",
                ["UseMigrations"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IProductRepository));
            services.RemoveAll(typeof(ICategoryRepository));
            services.RemoveAll(typeof(IPriceHistoryRepository));
            services.RemoveAll(typeof(IUserRepository));
            services.RemoveAll(typeof(IRefreshTokenRepository));
            services.RemoveAll(typeof(IAuditLogRepository));

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
        var client = CreateClient();
        var token = GenerateTestToken(Guid.NewGuid(), "test@example.com", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public string GenerateTestToken(Guid userId, string email, UserRole role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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
