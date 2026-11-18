namespace StepByStepCoding.Application.Models
{
    public sealed record AcademicReadScope(Guid UserId, bool IsAdmin, bool IsInstructor, bool IsStudent, DateTimeOffset Now);
}
