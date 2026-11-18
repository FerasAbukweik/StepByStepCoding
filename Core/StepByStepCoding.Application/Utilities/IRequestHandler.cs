namespace StepByStepCoding.Application.Utilities;

public interface IRequestHandler<TRequest, TResponse> where TRequest : class
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<TRequest> where TRequest : class
{
    Task Handle(TRequest request, CancellationToken cancellationToken);
}
