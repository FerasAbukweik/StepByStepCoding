namespace StepByStepCoding.Application.Utilities;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
    Task Send(IRequest request, CancellationToken cancellationToken);
}
