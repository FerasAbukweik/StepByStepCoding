using StepByStepCoding.Security.Seeding;
using StepByStepCoding.Security.Contracts.Repositories;
using StepByStepCoding.Security.Decorators;
using StepByStepCoding.Security.BackgroundServices;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Security.Repositories;
using StepByStepCoding.EntityFrameworkCore.ExtensionMethods;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Security.Models;
using StepByStepCoding.Security.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StepByStepCoding.Security.Options;

namespace StepByStepCoding.Security.ExtensionMethods
{
    public static class SecurityServicesExtensionMethods
    {
        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));

            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

            services.AddOptions<JwtOptions>()
                .Validate(jwt => !string.IsNullOrWhiteSpace(jwt.Issuer) && !string.IsNullOrWhiteSpace(jwt.Audience)
                    && Encoding.UTF8.GetByteCount(jwt.Key) >= 32
                    && jwt.AccessTokenLifeTime > 0 && jwt.RefreshTokenLifeTime > jwt.AccessTokenLifeTime,
                    "Jwt requires an issuer, audience, key of at least 32 bytes, and valid token lifetimes in minutes.")
                .ValidateOnStart();

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationSecurityDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("SecurityConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:SecurityConnection is required."),
                sql => sql.EnableRetryOnFailure()));

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationSecurityDbContext>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddEntityFrameworkServices();

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserLookupRepository, UserLookupRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserDirectory, UserDirectory>();
            services.AddScoped<IUserLookupService, UserLookupService>();

            services.AddScoped<IAdminSeeder, AdminSeeder>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<AuthService>();
            services.AddScoped<IAuthService>(provider => provider.GetRequiredService<AuthService>());
            services.AddScoped<IAccessTokenService, AccessTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ITokensService, TokensService>();

            return services;
        }

        public static IServiceCollection AddDecorators(this IServiceCollection services)
        {
            services.Decorate<IAccountService, AccountServiceLoggingDecorator>();
            services.Decorate<IAuthService, AuthServiceLoggingDecorator>();
            services.Decorate<IAccessTokenService, AccessTokenServiceLoggingDecorator>();
            services.Decorate<IRefreshTokenService, RefreshTokenServiceLoggingDecorator>();
            services.Decorate<ITokensService, TokensServiceLoggingDecorator>();

            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<ExpiredRefreshTokensCleanupService>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                    };
                });

            return services;
        }
    }
}
