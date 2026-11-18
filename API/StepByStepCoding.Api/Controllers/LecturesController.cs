using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Lectures.Queries.GetLectureDetail;
using StepByStepCoding.Application.Features.Lectures.Queries.GetLecturesList;
using StepByStepCoding.DTOs.Lectures;
using StepByStepCoding.Application.Features.Lectures.Commands.CreateLecture;
using StepByStepCoding.Application.Features.Lectures.Commands.UpdateLecture;
using StepByStepCoding.Application.Features.Lectures.Commands.DeleteLecture;
using StepByStepCoding.Application.Features.Lectures.Commands.StartLecture;
using StepByStepCoding.Application.Features.Lectures.Commands.EndLecture;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/lectures")]
    public sealed class LecturesController : ControllerBase
    {
        private readonly IMediator mediator;

        public LecturesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<LectureListDTO>>> GetList(
            [FromQuery] GetLecturesListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LectureDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetLectureDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost]
        public async Task<IActionResult> CreateLecture(CreateLectureDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new CreateLectureCommand
            {
                FileName = request.FileName,
                Language = request.Language,
                InitialCode = request.InitialCode,
                SectionId = request.SectionId,
                Title = request.Title,
                ScheduledStart = request.ScheduledStart
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
        public async Task<IActionResult> UpdateLecture(Guid id, UpdateLectureDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateLectureCommand
            {
                FileId = request.FileId,
                FileVersion = request.FileVersion,
                FileName = request.FileName,
                Language = request.Language,
                InitialCode = request.InitialCode,
                Id = id,
                Version = request.Version,
                Title = request.Title,
                ScheduledStart = request.ScheduledStart
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLecture(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteLectureCommand
            {
                Id = id,
                Version = version
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost("{id:guid}/start")]
        public async Task<IActionResult> StartLecture(Guid id, StartLectureDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new StartLectureCommand
            {
                Id = id,
                Version = request.Version
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost("{id:guid}/end")]
        public async Task<IActionResult> EndLecture(Guid id, EndLectureDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new EndLectureCommand
            {
                Id = id,
                Version = request.Version
            }, cancellationToken);

            return NoContent();
        }
    }
}
