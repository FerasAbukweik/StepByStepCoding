using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionDetail;
using StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionsList;
using StepByStepCoding.DTOs.Submissions;
using StepByStepCoding.Application.Features.Submissions.Commands.SubmitCode;
using StepByStepCoding.Application.Features.Submissions.Commands.GradeSubmission;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/submissions")]
    public sealed class SubmissionsController : ControllerBase
    {
        private readonly IMediator mediator;

        public SubmissionsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<SubmissionListDTO>>> GetList(
            [FromQuery] GetSubmissionsListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SubmissionDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetSubmissionDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = nameof(UserRole.Student))]
        [HttpPost]
        public async Task<IActionResult> SubmitCode(SubmitCodeDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new SubmitCodeCommand
            {
                AttemptId = request.AttemptId,
                QuestionId = request.QuestionId,
                SubmittedCode = request.SubmittedCode,
                Language = request.Language
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
        [HttpPost("{id:guid}/grade")]
        public async Task<IActionResult> GradeSubmission(Guid id, GradeSubmissionDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new GradeSubmissionCommand
            {
                Id = id,
                Version = request.Version,
                Score = request.Score,
                Feedback = request.Feedback
            }, cancellationToken);

            return NoContent();
        }
    }
}
