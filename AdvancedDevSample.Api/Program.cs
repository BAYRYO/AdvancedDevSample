using AdvancedDevSample.Api.Middlewares;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Seeders;
using AdvancedDevSample.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Sentry;

var builder = WebApplication.CreateBuilder(args);

// Configure Sentry for error monitoring, logging, and tracing
builder.WebHost.UseSentry(options =>
{
    options.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN")
                  ?? builder.Configuration["Sentry:Dsn"];
    options.Environment = builder.Environment.EnvironmentName;

    // Performance tracing - capture 100% in dev, 20% in production
    options.TracesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.2;

    // Profile sampling for performance insights
    options.ProfilesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.1;

    // Debug mode in development
    options.Debug = builder.Environment.IsDevelopment();

    // Privacy - don't send PII
    options.SendDefaultPii = false;

    // Always attach stack traces
    options.AttachStacktrace = true;

    // Breadcrumbs for debugging context
    options.MaxBreadcrumbs = 100;
    options.MinimumBreadcrumbLevel = LogLevel.Debug;

    // Capture all errors and warnings to Sentry
    options.MinimumEventLevel = LogLevel.Warning;

    // Auto session tracking
    options.AutoSessionTracking = true;

    // Release tracking
    options.Release = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
});

// Configure logging with Sentry integration
builder.Logging.AddSentry(options =>
{
    options.MinimumBreadcrumbLevel = LogLevel.Debug;
    options.MinimumEventLevel = LogLevel.Warning;
    options.InitializeSdk = false; // SDK already initialized above
});

// Configure SQLite database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=advanceddevsample.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register repositories
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<IPriceHistoryRepository, EfPriceHistoryRepository>();

// Register services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();

// Register database seeder
builder.Services.AddDatabaseSeeder();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Verify Sentry is initialized and send a test event in development
if (app.Environment.IsDevelopment() && SentrySdk.IsEnabled)
{
    app.Logger.LogInformation("Sentry SDK is enabled and initialized");
    SentrySdk.CaptureMessage("Sentry initialized successfully - Application starting", SentryLevel.Info);
}

// Ensure database is created and seeded
var seedDatabase = builder.Configuration.GetValue<bool>("SeedDatabase", true);
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    // Seed database in development (can be disabled via configuration)
    if (app.Environment.IsDevelopment() && seedDatabase)
    {
        await app.Services.SeedDatabaseAsync();
    }
}

// Sentry tracing middleware - must be early in pipeline to capture all requests
app.UseSentryTracing();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "AdvancedDevSample API v1");
    });
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

// Ensure Sentry flushes events on shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    SentrySdk.Flush(TimeSpan.FromSeconds(2));
});

app.Run();

public partial class Program { }
