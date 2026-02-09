using System.Net;
using System.Text.Json;
using AdvancedDevSample.Domain.Exceptions;
using Sentry;

namespace AdvancedDevSample.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Add request context as breadcrumb
            SentrySdk.AddBreadcrumb(
                message: $"{context.Request.Method} {context.Request.Path}",
                category: "http.request",
                level: BreadcrumbLevel.Info,
                data: new Dictionary<string, string>
                {
                    ["method"] = context.Request.Method,
                    ["path"] = context.Request.Path,
                    ["query"] = context.Request.QueryString.ToString()
                });

            // Configure scope with request context
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("http.method", context.Request.Method);
                scope.SetTag("http.path", context.Request.Path);

                // Add request ID for correlation
                if (context.TraceIdentifier != null)
                {
                    scope.SetTag("request.id", context.TraceIdentifier);
                }
            });

            try
            {
                await _next(context);

                // Log successful responses with status code
                SentrySdk.AddBreadcrumb(
                    message: $"Response {context.Response.StatusCode}",
                    category: "http.response",
                    level: BreadcrumbLevel.Info,
                    data: new Dictionary<string, string>
                    {
                        ["status_code"] = context.Response.StatusCode.ToString()
                    });
            }
            catch (InvalidCredentialsException ex)
            {
                _logger.LogWarning(ex, "Echec d'authentification: {Message}", ex.Message);
                await CaptureAndRespondAsync(context, ex, "authentication", SentryLevel.Warning,
                    StatusCodes.Status401Unauthorized, "Authentification echouee");
            }
            catch (UserAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, "Conflit utilisateur: {Message}", ex.Message);
                await CaptureAndRespondAsync(context, ex, "conflict", SentryLevel.Warning,
                    StatusCodes.Status409Conflict, "Conflit");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erreur métier: {Message}", ex.Message);
                await CaptureAndRespondAsync(context, ex, "domain", SentryLevel.Warning,
                    StatusCodes.Status400BadRequest, "Erreur métier");
            }
            catch (ApplicationServiceException ex)
            {
                _logger.LogWarning(ex, "Erreur applicative: {Message}", ex.Message);
                await CaptureAndRespondAsync(context, ex, "application", SentryLevel.Warning,
                    (int)ex.StatusCode, "Ressource introuvable",
                    new Dictionary<string, string> { ["status.code"] = ex.StatusCode.ToString() });
            }
            catch (InfrastructureException ex)
            {
                _logger.LogError(ex, "Erreur technique: {Message}", ex.Message);
                await CaptureAndRespondAsync(context, ex, "infrastructure", SentryLevel.Error,
                    (int)HttpStatusCode.InternalServerError, "Erreur technique");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Erreur inattendue: {Message}", ex.Message);
                await CaptureAndRespondAsync(context, ex, "unexpected", SentryLevel.Fatal,
                    (int)HttpStatusCode.InternalServerError, "Erreur serveur");
            }
        }

        private async Task CaptureAndRespondAsync(
            HttpContext context,
            Exception ex,
            string errorType,
            SentryLevel level,
            int statusCode,
            string title,
            Dictionary<string, string>? additionalTags = null)
        {
            // Capture exception with full context
            SentrySdk.CaptureException(ex, scope =>
            {
                scope.Level = level;
                scope.SetTag("error.type", errorType);
                scope.SetTag("http.status_code", statusCode.ToString());
                scope.SetTag("http.method", context.Request.Method);
                scope.SetTag("http.path", context.Request.Path);

                if (additionalTags != null)
                {
                    foreach (var tag in additionalTags)
                    {
                        scope.SetTag(tag.Key, tag.Value);
                    }
                }

                // Add exception fingerprint for better grouping
                scope.SetFingerprint(errorType, context.Request.Path.ToString(), ex.GetType().Name);

                // Add extra context data
                scope.SetExtra("request.path", context.Request.Path.ToString());
                scope.SetExtra("request.method", context.Request.Method);
                scope.SetExtra("request.query", context.Request.QueryString.ToString());
                scope.SetExtra("exception.type", ex.GetType().FullName);
            });

            // Add error breadcrumb
            SentrySdk.AddBreadcrumb(
                message: $"Error: {ex.Message}",
                category: "error",
                level: BreadcrumbLevel.Error,
                data: new Dictionary<string, string>
                {
                    ["type"] = errorType,
                    ["exception"] = ex.GetType().Name
                });

            // Send response
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            if (errorType == "infrastructure")
            {
                await context.Response.WriteAsJsonAsync(new { error = title });
            }
            else
            {
                await context.Response.WriteAsJsonAsync(new { title, detail = ex.Message });
            }
        }
    }
}
