using System.Net;
using Newtonsoft.Json;
using PPTRevive.Application.Common.Exceptions;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;

namespace PPTRevive.Web.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var innerExceptionMessage = ex.InnerException?.Message.ToString() ?? "None";
            _logger.LogError(ex, "Unhandled exception: {Message}. Inner Exception: {InnerException}", ex.Message, innerExceptionMessage);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ResponseBase response = new();

        switch (exception)
        {
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new ResponseBase
                {
                    Status = false,
                    Message = HttpStatusCode.NotFound.ToReadableString(),
                    // Raw exception detail is only exposed outside production.
                    Error = _env.IsProduction() ? "The requested resource was not found." : exception.Message
                };
            break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new ResponseBase
                {
                    Status = false,
                    Message = HttpStatusCode.InternalServerError.ToReadableString(),
                    Error = _env.IsProduction() ? "An unexpected error occurred. Please try again." : exception.Message
                };
            break;

            // Validation messages are user-facing and safe to return in all environments.
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new ResponseBase
                {
                    Status = false,
                    Message = HttpStatusCode.BadRequest.ToReadableString(),
                    Error = string.Join(" ", validationEx.Errors.SelectMany(kv => kv.Value))
                };
                break;
        }

        var jsonResponse = JsonConvert.SerializeObject(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}
