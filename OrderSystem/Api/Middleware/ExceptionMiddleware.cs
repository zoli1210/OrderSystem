using System.Net;
using System.Text.Json;

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
        catch (Exception ex)
        {
            var (message, statusCode) = ex switch
            {
                UnauthorizedAccessException => ("Forbidden", HttpStatusCode.Forbidden),
                InvalidOperationException => (ex.Message, HttpStatusCode.BadRequest),
                ArgumentException => (ex.Message, HttpStatusCode.BadRequest),
                _ => ("Internal server error", HttpStatusCode.InternalServerError),
            };

            await HandleException(context, message, statusCode);
        }
    }

    private static async Task HandleException(
        HttpContext context,
        string message,
        HttpStatusCode statusCode
    )
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new { message, statusCode = context.Response.StatusCode };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
