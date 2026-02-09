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
        IHeaderDictionary headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        if (context.Request.IsHttps && !_environment.IsDevelopment())
        {
            headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains";
        }

        string path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase))
        {
            headers.ContentSecurityPolicy =
                "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; " +
                "connect-src 'self'; font-src 'self' data:; object-src 'none'; frame-src 'none'; " +
                "frame-ancestors 'none'; base-uri 'self'; form-action 'self';";
        }
        else
        {
            headers.ContentSecurityPolicy =
                "default-src 'none'; object-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none';";
        }

        await _next(context);
    }
}
