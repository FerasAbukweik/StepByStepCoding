using FluentValidation.Results;

namespace StepByStepCoding.Application.Exceptions;

public class CustomValidationException : Exception
{
    public List<string> Errors { get; set; } = [];

    public CustomValidationException(ValidationResult validationResult)
    {
        Errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    public CustomValidationException(string message)
    {
        Errors.Add(message);
    }
}
