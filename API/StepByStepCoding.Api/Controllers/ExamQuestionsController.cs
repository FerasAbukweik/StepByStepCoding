using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionDetail;
using StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionsList;
using StepByStepCoding.DTOs.ExamQuestions;
using StepByStepCoding.Application.Features.ExamQuestions.Commands.CreateExamQuestion;
using StepByStepCoding.Application.Features.ExamQuestions.Commands.UpdateExamQuestion;
using StepByStepCoding.Application.Features.ExamQuestions.Commands.DeleteExamQuestion;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/examquestions")]
    public sealed class ExamQuestionsController : ControllerBase
    {
        private readonly IMediator mediator;

        public ExamQuestionsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<ExamQuestionListDTO>>> GetList(
            [FromQuery] GetExamQuestionsListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ExamQuestionDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetExamQuestionDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost]
        public async Task<IActionResult> CreateExamQuestion(CreateExamQuestionDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new CreateExamQuestionCommand
            {
                ExamId = request.ExamId,
                Title = request.Title,
                Description = request.Description,
                ImagePath = request.ImagePath,
                MaxScore = request.MaxScore,
                DisplayOrder = request.DisplayOrder
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
        public async Task<IActionResult> UpdateExamQuestion(Guid id, UpdateExamQuestionDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateExamQuestionCommand
            {
                Id = id,
                Version = request.Version,
                Title = request.Title,
                Description = request.Description,
                ImagePath = request.ImagePath,
                MaxScore = request.MaxScore,
                DisplayOrder = request.DisplayOrder
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteExamQuestion(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteExamQuestionCommand
            {
                Id = id,
                Version = version
            }, cancellationToken);

            return NoContent();
        }
    }
}
