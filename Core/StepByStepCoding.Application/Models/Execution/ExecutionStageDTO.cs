namespace StepByStepCoding.Application.Models.Execution
{
    public sealed class ExecutionStageDTO
    {
        public string Stdout { get; init; } = "";

        public string Stderr { get; init; } = "";

        public int? ExitCode { get; init; }

        public string? Signal { get; init; }

        public string? Status { get; init; }

        public string? Message { get; init; }

        public double? WallTimeMilliseconds { get; init; }
    }
}
