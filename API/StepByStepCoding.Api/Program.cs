using Serilog;
using StepByStepCoding.Application;
using StepByStepCoding.Infrastructure;
using StepByStepCoding.Persistence;
using StepByStepCoding;
using StepByStepCoding.Security;
using StepByStepCoding.MiddleWares;
using StepByStepCoding.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplicationServices()
    .AddPersistenceServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddSecurityServices(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseGlobalExceptionMiddleWare();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();
app.MapRealtime();

await app.SeedAdmin(app.Lifetime.ApplicationStopping);

app.Run();
