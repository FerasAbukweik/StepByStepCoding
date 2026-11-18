using StepByStepCoding.Security.Contracts.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Repositories;

namespace StepByStepCoding.Security.BackgroundServices
{
    public sealed class ExpiredRefreshTokensCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<ExpiredRefreshTokensCleanupService> logger;

        public ExpiredRefreshTokensCleanupService(IServiceScopeFactory scopeFactory,
            ILogger<ExpiredRefreshTokensCleanupService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            try
            {
                do
                {
                    try
                    {
                        // Hosted services are singletons; each run needs its own scoped DbContext.
                        await using var scope = scopeFactory.CreateAsyncScope();
                        var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

                        var deleted = await repository.DeleteExpired(DateTimeOffset.UtcNow, stoppingToken);

                        logger.LogInformation("Removed {Count} expired refresh tokens.", deleted);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "Expired refresh-token cleanup failed; it will run again next hour.");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal host shutdown while waiting for the next run.
            }
        }
    }
}
