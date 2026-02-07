namespace AdvancedDevSample.Api.Middlewares;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        if (context.Request.IsHttps && !_environment.IsDevelopment())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase))
        {
            headers["Content-Security-Policy"] =
                "default-src 'self'; img-src 'self' data: https:; style-src 'self' 'unsafe-inline'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; connect-src 'self' http: https:; " +
                "font-src 'self' data:; frame-ancestors 'none'; base-uri 'self';";
        }
        else
        {
            headers["Content-Security-Policy"] =
                "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none';";
        }

        await _next(context);
    }
}
