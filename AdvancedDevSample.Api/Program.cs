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
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();

// Register services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuditService>();

// Register infrastructure services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITransactionManager, EfTransactionManager>();

// Register database seeder
builder.Services.AddDatabaseSeeder();

// Configure JWT Settings
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
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
    options.Issuer = builder.Configuration["Jwt:Issuer"] ?? "AdvancedDevSample";
    options.Audience = builder.Configuration["Jwt:Audience"] ?? "AdvancedDevSample";
    options.ExpirationMinutes = builder.Configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
});

// Configure JWT Authentication
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AdvancedDevSample",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AdvancedDevSample",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization with role-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// Configure Rate Limiting for login endpoint
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed window rate limiter for login endpoint: 5 requests per minute per IP
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

builder.Services.AddControllers();

// Configure CORS for frontend applications
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            allowedOrigins = ["http://localhost:5173", "https://localhost:7173"];
        }

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(options =>
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

var app = builder.Build();

// Verify Sentry is initialized and send a test event in development
if (app.Environment.IsDevelopment() && SentrySdk.IsEnabled)
{
    app.Logger.LogInformation("Sentry SDK is enabled and initialized");
    SentrySdk.CaptureMessage("Sentry initialized successfully - Application starting", SentryLevel.Info);
}

// Ensure database is created and seeded
var seedDatabase = builder.Configuration.GetValue<bool>("SeedDatabase", true);
var useMigrations = builder.Configuration.GetValue<bool>("UseMigrations", true);
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (useMigrations && dbContext.Database.IsRelational())
    {
        var hasMigrations = dbContext.Database.GetMigrations().Any();
        if (hasMigrations)
        {
            await dbContext.Database.MigrateAsync();
        }
        else if (app.Environment.IsDevelopment())
        {
            app.Logger.LogWarning(
                "UseMigrations is enabled but no EF migrations were found. Falling back to EnsureCreated().");
            await EnsureCreatedWithSqliteRaceToleranceAsync(dbContext, app.Logger);
        }
        else
        {
            throw new InvalidOperationException(
                "UseMigrations is enabled but no EF migrations were found. " +
                "Generate and apply migrations before starting in non-development environments.");
        }
    }
    else
    {
        await EnsureCreatedWithSqliteRaceToleranceAsync(dbContext, app.Logger);
    }

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

// Keep exception handling early so auth/rate-limit failures are normalized.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

// Ensure Sentry flushes events on shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    SentrySdk.Flush(TimeSpan.FromSeconds(2));
});

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
        logger.LogDebug("Ignoring SQLite EnsureCreated race condition: {Message}", ex.Message);
    }
}

app.Run();

public partial class Program { }
