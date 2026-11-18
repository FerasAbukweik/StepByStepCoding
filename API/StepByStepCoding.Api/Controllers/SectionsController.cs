using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Sections.Queries.GetSectionDetail;
using StepByStepCoding.Application.Features.Sections.Queries.GetSectionsList;
using StepByStepCoding.DTOs.Sections;
using StepByStepCoding.Application.Features.Sections.Commands.CreateSection;
using StepByStepCoding.Application.Features.Sections.Commands.UpdateSection;
using StepByStepCoding.Application.Features.Sections.Commands.DeleteSection;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/sections")]
    public sealed class SectionsController : ControllerBase
    {
        private readonly IMediator mediator;

        public SectionsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<SectionListDTO>>> GetList(
            [FromQuery] GetSectionsListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SectionDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetSectionDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost]
        public async Task<IActionResult> CreateSection(CreateSectionDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new CreateSectionCommand
            {
                CourseId = request.CourseId,
                InstructorId = request.InstructorId,
                Name = request.Name
            }, cancellationToken);

            return CreatedAtAction(nameof(GetById), new
            {
                id
            }, new
            {
                id
            });
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSection(Guid id, UpdateSectionDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateSectionCommand
            {
                Id = id,
                Version = request.Version,
                InstructorId = request.InstructorId,
                Name = request.Name
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSection(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteSectionCommand
            {
                Id = id,
                Version = version
            }, cancellationToken);

            return NoContent();
        }
    }
}
