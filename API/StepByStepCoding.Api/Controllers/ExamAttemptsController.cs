using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptDetail;
using StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptsList;
using StepByStepCoding.DTOs.ExamAttempts;
using StepByStepCoding.Application.Features.ExamAttempts.Commands.StartExamAttempt;
using StepByStepCoding.Application.Features.ExamAttempts.Commands.FinishExamAttempt;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/examattempts")]
    public sealed class ExamAttemptsController : ControllerBase
    {
        private readonly IMediator mediator;

        public ExamAttemptsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<ExamAttemptListDTO>>> GetList(
            [FromQuery] GetExamAttemptsListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ExamAttemptDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetExamAttemptDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = nameof(UserRole.Student))]
        [HttpPost]
        public async Task<IActionResult> StartExamAttempt(StartExamAttemptDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new StartExamAttemptCommand
            {
                ExamId = request.ExamId
            }, cancellationToken);

            return CreatedAtAction(nameof(GetById), new
            {
                id
            }, new
            {
                id
            });
        }

        [Authorize(Roles = nameof(UserRole.Student))]
        [HttpPost("{id:guid}/finish")]
        public async Task<IActionResult> FinishExamAttempt(Guid id, FinishExamAttemptDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new FinishExamAttemptCommand
            {
                Id = id,
                Version = request.Version
            }, cancellationToken);

            return NoContent();
        }
    }
}
