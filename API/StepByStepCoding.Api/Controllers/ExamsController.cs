using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Exams.Queries.GetExamDetail;
using StepByStepCoding.Application.Features.Exams.Queries.GetExamsList;
using StepByStepCoding.DTOs.Exams;
using StepByStepCoding.Application.Features.Exams.Commands.CreateExam;
using StepByStepCoding.Application.Features.Exams.Commands.UpdateExam;
using StepByStepCoding.Application.Features.Exams.Commands.DeleteExam;
using StepByStepCoding.Application.Features.Exams.Commands.PublishExam;
using StepByStepCoding.Application.Features.Exams.Commands.CloseExam;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/exams")]
    public sealed class ExamsController : ControllerBase
    {
        private readonly IMediator mediator;

        public ExamsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<ExamListDTO>>> GetList(
            [FromQuery] GetExamsListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ExamDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetExamDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost]
        public async Task<IActionResult> CreateExam(CreateExamDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new CreateExamCommand
            {
                SectionId = request.SectionId,
                Title = request.Title,
                Instructions = request.Instructions,
                StartsAt = request.StartsAt,
                Deadline = request.Deadline,
                DurationMinutes = request.DurationMinutes
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
        public async Task<IActionResult> UpdateExam(Guid id, UpdateExamDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateExamCommand
            {
                Id = id,
                Version = request.Version,
                Title = request.Title,
                Instructions = request.Instructions,
                StartsAt = request.StartsAt,
                Deadline = request.Deadline,
                DurationMinutes = request.DurationMinutes
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteExam(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteExamCommand
            {
                Id = id,
                Version = version
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> PublishExam(Guid id, PublishExamDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new PublishExamCommand
            {
                Id = id,
                Version = request.Version
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> CloseExam(Guid id, CloseExamDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new CloseExamCommand
            {
                Id = id,
                Version = request.Version
            }, cancellationToken);

            return NoContent();
        }
    }
}
