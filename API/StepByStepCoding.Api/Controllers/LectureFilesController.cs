using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.RateLimiting;
using StepByStepCoding.Application.Features.Files.Commands.RunFile;
using StepByStepCoding.Application.Models.Execution;
using StepByStepCoding.DTOs.Files;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;
using StepByStepCoding.Application.Features.Files.Queries.GetLectureFilesList;
using StepByStepCoding.DTOs.LectureFiles;
using StepByStepCoding.Application.Features.Files.Commands.CreateLectureFile;
using StepByStepCoding.Application.Features.Files.Commands.UpdateLectureFile;
using StepByStepCoding.Application.Features.Files.Commands.DeleteLectureFile;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/lecturefiles")]
    [Route("api/files")]
    public sealed class LectureFilesController : ControllerBase
    {
        private readonly IMediator mediator;

        public LectureFilesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<LectureFileListDTO>>> GetList(
            [FromQuery] GetLectureFilesListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FileDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetFileDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [HttpPost("{id:guid}/run")]
        [EnableRateLimiting("code-execution")]
        [RequestSizeLimit(700000)]
        public async Task<ActionResult<CodeExecutionResultDTO>> RunCode(Guid id, RunFileDTO request, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new RunFileCommand
            {
                FileId = id,
                Code = request.Code,
                Stdin = request.Stdin
            }, cancellationToken));
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost]
        public async Task<IActionResult> CreateLectureFile(CreateLectureFileDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new CreateLectureFileCommand
            {
                LectureId = request.LectureId,
                FileName = request.FileName,
                Language = request.Language,
                SavedContent = request.SavedContent,
                SavedRevision = request.SavedRevision
            }, cancellationToken);

            return CreatedAtAction(nameof(GetById), new
            {
                id
            }, new
            {
                id
            });
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateLectureFile(Guid id, UpdateLectureFileDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateLectureFileCommand
            {
                Id = id,
                Version = request.Version,
                FileName = request.FileName,
                Language = request.Language,
                SavedContent = request.SavedContent,
                SavedRevision = request.SavedRevision
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLectureFile(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteLectureFileCommand
            {
                Id = id,
                Version = version
            }, cancellationToken);

            return NoContent();
        }
    }
}
