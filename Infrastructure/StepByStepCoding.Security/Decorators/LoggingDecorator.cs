using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace StepByStepCoding.Security.Decorators
{
    public abstract class LoggingDecorator
    {
        private readonly ILogger logger;
        private readonly bool isBoundary;

        protected LoggingDecorator(ILogger logger, bool isBoundary = false)
        {
            this.logger = logger;
            this.isBoundary = isBoundary;
        }

        protected async Task<T> LogOperation<T>(
            string operation,
            Func<Task<T>> execute,
            Func<T, bool>? isSuccessful = null,
            CancellationToken cancellationToken = default)
        {
            var started = Stopwatch.GetTimestamp();
            logger.LogDebug("{Operation} started.", operation);

            try
            {
                var result = await execute();
                var succeeded = isSuccessful?.Invoke(result) ?? true;
                var level = isBoundary
                    ? succeeded ? LogLevel.Information : LogLevel.Warning
                    : LogLevel.Debug;

                // Log metadata only: request DTOs and results can contain passwords or tokens.
                logger.Log(level, "{Operation} completed with {Outcome} in {ElapsedMilliseconds} ms.",
                    operation, succeeded ? "Success" : "Rejected", Stopwatch.GetElapsedTime(started).TotalMilliseconds);

                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("{Operation} cancelled after {ElapsedMilliseconds} ms.",
                    operation, Stopwatch.GetElapsedTime(started).TotalMilliseconds);

                throw;
            }
            catch (Exception exception)
            {
                // The auth boundary records errors; nested token decorators stay at Debug to avoid duplicate errors.
                logger.Log(isBoundary ? LogLevel.Error : LogLevel.Debug,
                    "{Operation} failed with {ExceptionType} after {ElapsedMilliseconds} ms.",
                    operation, exception.GetType().Name, Stopwatch.GetElapsedTime(started).TotalMilliseconds);

                throw;
            }
        }
    }
}
