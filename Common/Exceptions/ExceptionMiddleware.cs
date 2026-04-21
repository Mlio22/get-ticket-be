using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Common.Exceptions;

/// <summary>
/// Middleware that catches AppExceptions and converts them to JSON HTTP responses.
/// Register with: app.UseMiddleware&lt;ExceptionMiddleware&gt;();
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await WriteErrorResponse(context, ex.StatusCode, ex.Message);
        }
        catch (Exception)
        {
            await WriteErrorResponse(
                context,
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred."
            );
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string message
    )
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var body = JsonSerializer.Serialize(
            new
            {
                isOk = false,
                errorMessage = message,
                statusCode,
            }
        );

        await context.Response.WriteAsync(body);
    }
}
