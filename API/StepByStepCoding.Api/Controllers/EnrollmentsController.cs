using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentDetail;
using StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentsList;
using StepByStepCoding.DTOs.Enrollments;
using StepByStepCoding.Application.Features.Enrollments.Commands.CreateEnrollment;
using StepByStepCoding.Application.Features.Enrollments.Commands.UpdateEnrollment;
using StepByStepCoding.Application.Features.Enrollments.Commands.DeleteEnrollment;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/enrollments")]
    public sealed class EnrollmentsController : ControllerBase
    {
        private readonly IMediator mediator;

        public EnrollmentsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<EnrollmentListDTO>>> GetList(
            [FromQuery] GetEnrollmentsListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EnrollmentDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetEnrollmentDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpPost]
        public async Task<IActionResult> CreateEnrollment(CreateEnrollmentDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new CreateEnrollmentCommand
            {
                SectionId = request.SectionId,
                StudentId = request.StudentId
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
        public async Task<IActionResult> UpdateEnrollment(Guid id, UpdateEnrollmentDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateEnrollmentCommand
            {
                Id = id,
                Version = request.Version,
                Active = request.Active
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Instructor)}")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEnrollment(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteEnrollmentCommand
            {
                Id = id,
                Version = version
            }, cancellationToken);

            return NoContent();
        }
    }
}
