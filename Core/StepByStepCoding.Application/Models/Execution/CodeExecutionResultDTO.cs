namespace StepByStepCoding.Application.Models.Execution
{
    public sealed class CodeExecutionResultDTO
    {
        public required string Language { get; init; }

        public required string Version { get; init; }

        public ExecutionStageDTO? Compile { get; init; }

        public ExecutionStageDTO? Run { get; init; }
    }
}
