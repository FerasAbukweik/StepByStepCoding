using StepByStepCoding.Domain.Exceptions;
using StepByStepCoding.Application.Exceptions;
using System.Text.Json;

namespace StepByStepCoding.MiddleWares;

public class GlobalExceptionMiddleWare(RequestDelegate next,
    ILogger<GlobalExceptionMiddleWare> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }

        catch (CustomValidationException exception)
        {
            if (context.Response.HasStarted)
                throw;

            logger.LogWarning("Request validation failed with {ErrorCount} errors.", exception.Errors.Count);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var response = new
            {
                errors = exception.Errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response), context.RequestAborted);
        }
        catch (Exception exception) when (exception is NotFoundException or ForbiddenException
            or ConflictException or BusinessRuleException or UnauthenticatedException or CodeExecutionUnavailableException)
        {
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = exception switch
            {
                CodeExecutionUnavailableException => StatusCodes.Status503ServiceUnavailable,
                NotFoundException => StatusCodes.Status404NotFound,
                ForbiddenException => StatusCodes.Status403Forbidden,
                ConflictException => StatusCodes.Status409Conflict,
                UnauthenticatedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = exception.Message
            }), context.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var res = new
            {
                message = "An unexpected error occurred. Please try again later." + ex.Message + ex.InnerException?.Message // temp for testing
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(res), context.RequestAborted);
        }
    }
}

public static class GlobalExceptionMiddleWareExtensionMethods
{
    public static IApplicationBuilder UseGlobalExceptionMiddleWare(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleWare>();
    }
}
