namespace StepByStepCoding.Application.Exceptions
{
    public sealed class UnauthenticatedException : Exception
    {
        public UnauthenticatedException(string message) : base(message) { }
    }
}
