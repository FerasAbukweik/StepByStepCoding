using StepByStepCoding.Application.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using StepByStepCoding.Application.Enums;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.Application.Features.Courses.Queries.GetCourseDetail;
using StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList;
using StepByStepCoding.DTOs.Courses;
using StepByStepCoding.Application.Features.Courses.Commands.CreateCourse;
using StepByStepCoding.Application.Features.Courses.Commands.UpdateCourse;
using StepByStepCoding.Application.Features.Courses.Commands.DeleteCourse;

namespace StepByStepCoding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/courses")]
    public sealed class CoursesController : ControllerBase
    {
        private readonly IMediator mediator;

        public CoursesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedDTO<CourseListDTO>>> GetList(
            [FromQuery] GetCoursesListQuery query, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CourseDetailDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetCourseDetailQuery
            {
                Id = id
            }, cancellationToken));
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourseDTO request, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(new CreateCourseCommand
            {
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
        public async Task<IActionResult> UpdateCourse(Guid id, UpdateCourseDTO request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateCourseCommand
            {
                Id = id,
                Version = request.Version,
                Name = request.Name
            }, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCourse(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteCourseCommand
            {
                Id = id,
                Version = version
            }, cancellationToken);

            return NoContent();
        }
    }
}
