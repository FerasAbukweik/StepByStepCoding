using StepByStepCoding.Application.Models.Execution;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Files.Commands.RunFile
{
    // Executions neither cache their results nor invalidate persisted file data.
    public sealed class RunFileCommand : IRequest<CodeExecutionResultDTO>
    {
        public required Guid FileId { get; init; }

        public required string Code { get; init; }

        public string Stdin { get; init; } = "";
    }
}
