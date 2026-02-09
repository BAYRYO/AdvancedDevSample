using System.Text;
using System.Threading.RateLimiting;
using AdvancedDevSample.Api.Middlewares;
using AdvancedDevSample.Application.Configuration;
using AdvancedDevSample.Application.Interfaces;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Seeders;
using AdvancedDevSample.Infrastructure.Repositories;
using AdvancedDevSample.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Sentry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
const string JwtIssuerAudience = "AdvancedDevSample";

ConfigureSentry(builder);
ConfigureLogging(builder);
ConfigureDatabase(builder);
RegisterApplicationServices(builder.Services);
RegisterInfrastructureServices(builder.Services);
ConfigureJwt(builder, JwtIssuerAudience);
ConfigureAuthAndAuthorization(builder);
ConfigureRateLimiting(builder.Services);
ConfigureCors(builder.Services, builder.Configuration);
ConfigureSwagger(builder.Services);
builder.Services.AddControllers();
builder.Services.AddDatabaseSeeder();

WebApplication app = builder.Build();

LogSentryInitialization(app);
await InitializeDatabaseAsync(app);
ConfigurePipeline(app);
RegisterShutdownHandlers(app);

await app.RunAsync();

static void ConfigureSentry(WebApplicationBuilder builder)
{
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN")
                      ?? builder.Configuration["Sentry:Dsn"];
        options.Environment = builder.Environment.EnvironmentName;
        options.TracesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.2;
        options.ProfilesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.1;
        options.Debug = builder.Environment.IsDevelopment();
        options.SendDefaultPii = false;
        options.AttachStacktrace = true;
        options.MaxBreadcrumbs = 100;
        options.MinimumBreadcrumbLevel = LogLevel.Debug;
        options.MinimumEventLevel = LogLevel.Warning;
        options.AutoSessionTracking = true;
        options.Release = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    });
}

static void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.AddSentry(options =>
    {
        options.MinimumBreadcrumbLevel = LogLevel.Debug;
        options.MinimumEventLevel = LogLevel.Warning;
        options.InitializeSdk = false;
    });
}

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=advanceddevsample.db";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
}

static void RegisterApplicationServices(IServiceCollection services)
{
    services.AddScoped<ProductService>();
    services.AddScoped<CategoryService>();
    services.AddScoped<AuthService>();
    services.AddScoped<UserService>();
    services.AddScoped<AuditService>();
}

static void RegisterInfrastructureServices(IServiceCollection services)
{
    services.AddScoped<IProductRepository, EfProductRepository>();
    services.AddScoped<ICategoryRepository, EfCategoryRepository>();
    services.AddScoped<IPriceHistoryRepository, EfPriceHistoryRepository>();
    services.AddScoped<IUserRepository, EfUserRepository>();
    services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
    services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();

    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<ITransactionManager, EfTransactionManager>();
}

static void ConfigureJwt(WebApplicationBuilder builder, string issuerAudience)
{
    string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? builder.Configuration["JWT_SECRET"]
        ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set. Please set a secure secret key (minimum 32 characters).");

    if (jwtSecret.Length < 32)
    {
        throw new InvalidOperationException(
            "JWT_SECRET is too short. Please use a secure secret key with at least 32 characters.");
    }

    builder.Services.Configure<JwtSettings>(options =>
    {
        options.Secret = jwtSecret;
        options.Issuer = builder.Configuration["Jwt:Issuer"] ?? issuerAudience;
        options.Audience = builder.Configuration["Jwt:Audience"] ?? issuerAudience;
        options.ExpirationMinutes = builder.Configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? issuerAudience,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? issuerAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });
}

static void ConfigureAuthAndAuthorization(WebApplicationBuilder builder)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
        .AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
}

static void ConfigureRateLimiting(IServiceCollection services)
{
    services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy("login", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
    });
}

static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
{
    string[] configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    string[] allowedOrigins = configuredOrigins.Length == 0
        ? ["http://localhost:5173", "https://localhost:7173"]
        : configuredOrigins;

    services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        });
    });
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "AdvancedDevSample API",
            Version = "v1",
            Description = "API with JWT Bearer authentication"
        });

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Enter your JWT token in the format: Bearer <your-jwt-token>"
        });

        options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document, null),
                []
            }
        });
    });
}

static void LogSentryInitialization(WebApplication app)
{
    if (!app.Environment.IsDevelopment() || !SentrySdk.IsEnabled)
    {
        return;
    }

    app.Logger.LogInformation("Sentry SDK is enabled and initialized");
    SentrySdk.CaptureMessage("Sentry initialized successfully - Application starting", SentryLevel.Info);
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    bool seedDatabase = app.Configuration.GetValue<bool>("SeedDatabase", true);
    bool useMigrations = app.Configuration.GetValue<bool>("UseMigrations", true);

    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (useMigrations && dbContext.Database.IsRelational())
    {
        await ApplyMigrationsOrFallbackAsync(app, dbContext);
    }
    else
    {
        await EnsureCreatedWithSqliteRaceToleranceAsync(dbContext, app.Logger);
    }

    if (app.Environment.IsDevelopment() && seedDatabase)
    {
        await app.Services.SeedDatabaseAsync();
    }
}

static async Task ApplyMigrationsOrFallbackAsync(WebApplication app, AppDbContext dbContext)
{
    bool hasMigrations = dbContext.Database.GetMigrations().Any();
    if (hasMigrations)
    {
        await dbContext.Database.MigrateAsync();
        return;
    }

    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogWarning(
            "UseMigrations is enabled but no EF migrations were found. Falling back to EnsureCreated().");
        await EnsureCreatedWithSqliteRaceToleranceAsync(dbContext, app.Logger);
        return;
    }

    throw new InvalidOperationException(
        "UseMigrations is enabled but no EF migrations were found. " +
        "Generate and apply migrations before starting in non-development environments.");
}

static void ConfigurePipeline(WebApplication app)
{
    app.UseSentryTracing();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "AdvancedDevSample API v1");
        });
        app.MapScalarApiReference(options =>
        {
            options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("Frontend");

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();
}

static void RegisterShutdownHandlers(WebApplication app)
{
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        SentrySdk.Flush(TimeSpan.FromSeconds(2));
    });
}

static async Task EnsureCreatedWithSqliteRaceToleranceAsync(AppDbContext dbContext, ILogger logger)
{
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
    catch (SqliteException ex) when (
        ex.SqliteErrorCode == 1 &&
        ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
    {
        logger.LogDebug(ex, "Ignoring SQLite EnsureCreated race condition: {Message}", ex.Message);
    }
}

public partial class Program
{
    protected Program()
    {
    }
}
