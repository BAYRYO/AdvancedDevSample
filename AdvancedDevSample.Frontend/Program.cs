using AdvancedDevSample.Frontend;
using AdvancedDevSample.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<BrowserStorageService>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<FrontendAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<FrontendAuthStateProvider>());
builder.Services.AddScoped<AuthTokenHandler>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddHttpClient("ApiNoAuth", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthTokenHandler>();

await builder.Build().RunAsync();
