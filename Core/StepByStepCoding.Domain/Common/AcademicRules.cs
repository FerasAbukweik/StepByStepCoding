using StepByStepCoding.Domain.Exceptions;

namespace StepByStepCoding.Domain.Common
{
    public static class AcademicRules
    {
        public static string Text(string? value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > maxLength)
                throw new BusinessRuleException($"{name} is required and must not exceed {maxLength} characters.");

            return value.Trim();
        }

        public static string? OptionalText(string? value, string name, int maxLength)
        {
            if (value?.Trim().Length > maxLength)
                throw new BusinessRuleException($"{name} must not exceed {maxLength} characters.");

            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static Guid Reference(Guid value, string name)
        {
            if (value == Guid.Empty)
                throw new BusinessRuleException($"{name} is required.");

            return value;
        }

        public static void Require(bool condition, string message)
        {
            if (!condition)
                throw new BusinessRuleException(message);
        }

        public static void Score(decimal value, bool allowZero = false)
        {
            Require((allowZero ? value >= 0 : value > 0) && value <= 10000 && decimal.Round(value, 2) == value,
                "Scores must be within range and have at most two decimal places.");
        }

        public static string Code(string? code)
        {
            Require(!string.IsNullOrWhiteSpace(code) && code.Length <= 100000, "Code is required and must not exceed 100000 characters.");

            return code!;
        }
    }
}
