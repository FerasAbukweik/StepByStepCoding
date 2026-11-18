using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Execution;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Execution;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.Features.Files.Commands.RunFile
{
    public sealed class RunFileCommandHandler : IRequestHandler<RunFileCommand, CodeExecutionResultDTO>
    {
        private readonly IFileRepository files;
        private readonly IAcademicAccess access;
        private readonly ICodeExecutionService execution;

        public RunFileCommandHandler(IFileRepository files, IAcademicAccess access, ICodeExecutionService execution)
        {
            this.files = files;
            this.access = access;
            this.execution = execution;
        }

        public async Task<CodeExecutionResultDTO> Handle(RunFileCommand request, CancellationToken cancellationToken)
        {
            var file = await files.GetWithEnrollment(request.FileId, access.CurrentScope().UserId, cancellationToken)
                ?? throw new NotFoundException("File was not found.");
            access.ReadFile(file);

            // Run the current editor draft without overwriting the saved file.
            return await execution.Execute(file.Language, file.FileName, request.Code, request.Stdin, cancellationToken);
        }
    }
}
