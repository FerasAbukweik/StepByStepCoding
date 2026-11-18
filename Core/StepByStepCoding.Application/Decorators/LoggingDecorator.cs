using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Decorators
{
    public sealed class LoggingDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : class
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly ILogger<LoggingDecorator<TRequest, TResponse>> logger;

        public LoggingDecorator(IRequestHandler<TRequest, TResponse> inner, ILogger<LoggingDecorator<TRequest, TResponse>> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var operation = $"{nameof(LoggingDecorator<TRequest, TResponse>)}.{nameof(Handle)}";
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();
            logger.LogInformation("{Operation} Handling {Request}", operation, requestName);

            try
            {
                var result = await inner.Handle(request, cancellationToken);
                logger.LogInformation("{Operation} Handled {Request} in {Elapsed} ms",
                    operation, requestName, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("{Operation} {Request} cancelled after {Elapsed} ms",
                    operation, requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError("{Operation} {Request} failed after {Elapsed} ms ({ExceptionType})",
                    operation, requestName, stopwatch.ElapsedMilliseconds, exception.GetType().Name);
                throw;
            }
        }
    }

    public sealed class LoggingDecorator<TRequest> : IRequestHandler<TRequest>
        where TRequest : class
    {
        private readonly IRequestHandler<TRequest> inner;
        private readonly ILogger<LoggingDecorator<TRequest>> logger;

        public LoggingDecorator(IRequestHandler<TRequest> inner, ILogger<LoggingDecorator<TRequest>> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public async Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            var operation = $"{nameof(LoggingDecorator<TRequest>)}.{nameof(Handle)}";
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();
            logger.LogInformation("{Operation} Handling {Request}", operation, requestName);

            try
            {
                await inner.Handle(request, cancellationToken);
                logger.LogInformation("{Operation} Handled {Request} in {Elapsed} ms",
                    operation, requestName, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("{Operation} {Request} cancelled after {Elapsed} ms",
                    operation, requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError("{Operation} {Request} failed after {Elapsed} ms ({ExceptionType})",
                    operation, requestName, stopwatch.ElapsedMilliseconds, exception.GetType().Name);
                throw;
            }
        }
    }
}
