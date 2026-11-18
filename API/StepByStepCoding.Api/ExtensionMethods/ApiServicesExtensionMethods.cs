using StepByStepCoding.Contracts.Cookie;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Services;
using StepByStepCoding.Options;

namespace StepByStepCoding.ExtensionMethods;

public static class ApiServicesExtensionMethods
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }

    public static IServiceCollection AddAuthenticationCookies(this IServiceCollection services)
    {
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<CookieKeys>>((options, cookieKeys) =>
            {
                options.Events.OnMessageReceived = context =>
                {
                    // Let the bearer handler use the header when both transports are present.
                    if (!context.Request.Headers.ContainsKey("Authorization")
                        && !string.IsNullOrWhiteSpace(cookieKeys.Value.AccessToken)
                        && context.Request.Cookies.TryGetValue(cookieKeys.Value.AccessToken, out var accessToken))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                };
            });

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICookieService, CookieService>();

        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CookieKeys>(configuration.GetSection(CookieKeys.SectionName));
        services.AddOptions<CookieKeys>()
            .Validate(keys => IsCookieName(keys.AccessToken) && IsCookieName(keys.RefreshToken)
                && !string.Equals(keys.AccessToken, keys.RefreshToken, StringComparison.Ordinal),
                "Token cookie names must be valid and distinct.")
            .ValidateOnStart();

        return services;
    }
    private static bool IsCookieName(string? name)
    {
        return !string.IsNullOrEmpty(name) && name.Length <= 128
            && name.All(character => char.IsAsciiLetterOrDigit(character) || "!#$%&'*+-.^_`|~".Contains(character));
    }
}
