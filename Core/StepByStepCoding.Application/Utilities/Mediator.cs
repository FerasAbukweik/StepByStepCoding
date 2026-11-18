using System.Reflection;
using System.Runtime.ExceptionServices;
using FluentValidation;
using StepByStepCoding.Application.Exceptions;

namespace StepByStepCoding.Application.Utilities
{
    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            await ApplyValidation(request, cancellationToken);

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var task = (Task<TResponse>)InvokeHandler(handlerType, request, cancellationToken);

            return await task;
        }

        public async Task Send(IRequest request, CancellationToken cancellationToken)
        {
            await ApplyValidation(request, cancellationToken);

            var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());

            await (Task)InvokeHandler(handlerType, request, cancellationToken);
        }

        private object InvokeHandler(Type handlerType, object request, CancellationToken cancellationToken)
        {
            var handler = serviceProvider.GetService(handlerType)
                ?? throw new MediatorException($"Handler was not found for {handlerType}.");
            var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<object>.Handle))
                ?? throw new MediatorException($"Handle method was not found for {handlerType}.");

            try
            {
                // Invoke the interface method so explicit handler implementations work too.
                return handleMethod.Invoke(handler, [request, cancellationToken])
                    ?? throw new MediatorException($"Handler {handlerType} returned no task.");
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                // Preserve the original exception type for the API's error mapping.
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }

        private async Task ApplyValidation(object request, CancellationToken cancellationToken)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
            var validator = (IValidator?)serviceProvider.GetService(validatorType);

            if (validator is null)
                return;

            var result = await validator.ValidateAsync(new ValidationContext<object>(request), cancellationToken);

            if (!result.IsValid)
                throw new CustomValidationException(result);
        }
    }
}
