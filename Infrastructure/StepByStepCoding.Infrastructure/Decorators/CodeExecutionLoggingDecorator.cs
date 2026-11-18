using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Execution;
using StepByStepCoding.Application.Models.Execution;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Infrastructure.Services;

namespace StepByStepCoding.Infrastructure.Decorators
{
    public sealed class CodeExecutionLoggingDecorator : ICodeExecutionService
    {
        private readonly ICodeExecutionService inner;
        private readonly ILogger<CodeExecutionLoggingDecorator> logger;

        public CodeExecutionLoggingDecorator(ICodeExecutionService inner, ILogger<CodeExecutionLoggingDecorator> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public async Task<CodeExecutionResultDTO> Execute(ProgrammingLanguage language, string fileName, string code,
            string stdin, CancellationToken cancellationToken = default)
        {
            var started = Stopwatch.GetTimestamp();
            var result = await inner.Execute(language, fileName, code, stdin, cancellationToken);
            // Student source, stdin and output stay out of application logs.
            logger.LogDebug("{Service}.{Method} Completed {Language} in {ElapsedMilliseconds} ms",
                nameof(CodeExecutionService), nameof(Execute), language, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return result;
        }
    }
}
