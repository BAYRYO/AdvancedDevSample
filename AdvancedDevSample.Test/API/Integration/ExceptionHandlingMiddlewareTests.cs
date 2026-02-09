using System.Text.Json;
using AdvancedDevSample.Api.Middlewares;
using AdvancedDevSample.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdvancedDevSample.Test.API.Integration;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Invoke_When_ApplicationServiceException_Is_Thrown_Should_Use_Exception_Status_Code()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ApplicationServiceException("resource missing", System.Net.HttpStatusCode.NotFound),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        DefaultHttpContext context = CreateHttpContext();

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        JsonElement payload = await ReadJsonAsync(context);
        Assert.Equal("Ressource introuvable", payload.GetProperty("title").GetString());
        Assert.Equal("resource missing", payload.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Invoke_When_InfrastructureException_Is_Thrown_Should_Hide_Exception_Message()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InfrastructureException("db exploded"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        DefaultHttpContext context = CreateHttpContext();

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        JsonElement payload = await ReadJsonAsync(context);
        Assert.Equal("Erreur technique", payload.GetProperty("error").GetString());
        Assert.False(payload.TryGetProperty("detail", out _));
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using JsonDocument json = await JsonDocument.ParseAsync(context.Response.Body);
        return json.RootElement.Clone();
    }
}
