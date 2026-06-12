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
        catch (NotFoundException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            await HandleException(context, "Internal server error", HttpStatusCode.InternalServerError);
        }
    }

    private static async Task HandleException(HttpContext context, string message, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            message,
            statusCode = context.Response.StatusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}