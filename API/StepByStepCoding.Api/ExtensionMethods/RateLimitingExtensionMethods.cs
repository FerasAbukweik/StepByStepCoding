using Microsoft.AspNetCore.RateLimiting;

namespace StepByStepCoding.ExtensionMethods
{
    public static class RateLimitingExtensionMethods
    {
        public static IServiceCollection AddRequestLimits(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddConcurrencyLimiter("code-execution", limiter =>
                {
                    limiter.PermitLimit = 2;
                    limiter.QueueLimit = 0;
                });
                options.OnRejected = async (context, cancellationToken) =>
                    await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Code execution is busy. Please try again shortly." }, cancellationToken);
            });

            return services;
        }
    }
}
