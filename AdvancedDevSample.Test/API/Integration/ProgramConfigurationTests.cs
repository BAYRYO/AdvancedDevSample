using System.Reflection;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AdvancedDevSample.Test.API.Integration;

public class ProgramConfigurationTests
{
    [Fact]
    public async Task ConfigureCors_WithEmptyOrigins_UsesDefaultFrontendOrigins()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        InvokeProgramMethod("ConfigureCors", services, configuration);

        using var provider = services.BuildServiceProvider();
        var corsPolicyProvider = provider.GetRequiredService<ICorsPolicyProvider>();
        var policy = await corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), "Frontend");

        Assert.NotNull(policy);
        Assert.Contains("http://localhost:5173", policy!.Origins);
        Assert.Contains("https://localhost:7173", policy.Origins);
    }

    [Fact]
    public async Task ConfigureCors_WithConfiguredOrigins_UsesConfiguredValues()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://app.example.com",
                ["Cors:AllowedOrigins:1"] = "https://admin.example.com"
            })
            .Build();

        InvokeProgramMethod("ConfigureCors", services, configuration);

        using var provider = services.BuildServiceProvider();
        var corsPolicyProvider = provider.GetRequiredService<ICorsPolicyProvider>();
        var policy = await corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), "Frontend");

        Assert.NotNull(policy);
        Assert.Equal(2, policy!.Origins.Count);
        Assert.Contains("https://app.example.com", policy.Origins);
        Assert.Contains("https://admin.example.com", policy.Origins);
    }

    [Fact]
    public void ConfigureRateLimiting_ShouldRegisterLoginPolicyAnd429StatusCode()
    {
        var services = new ServiceCollection();

        InvokeProgramMethod("ConfigureRateLimiting", services);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        Assert.Equal(StatusCodes.Status429TooManyRequests, options.RejectionStatusCode);
    }

    private static object? InvokeProgramMethod(string methodName, params object[] args)
    {
        var method = typeof(Program)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .SingleOrDefault(m => m.Name.Contains($"g__{methodName}|", StringComparison.Ordinal));
        Assert.NotNull(method);
        return method!.Invoke(null, args);
    }
}
