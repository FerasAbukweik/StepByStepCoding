namespace StepByStepCoding.Application.Contracts.Persistence
{
    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : class
    {
    }
}
