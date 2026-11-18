using StepByStepCoding.Application.Models.Execution;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Contracts.Execution
{
    public interface ICodeExecutionService
    {
        Task<CodeExecutionResultDTO> Execute(ProgrammingLanguage language, string fileName, string code,
            string stdin, CancellationToken cancellationToken = default);
    }
}
