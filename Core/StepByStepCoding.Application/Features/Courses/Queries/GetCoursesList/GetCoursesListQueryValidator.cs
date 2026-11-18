using FluentValidation;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList
{
    public sealed class GetCoursesListQueryValidator : AbstractValidator<GetCoursesListQuery>
    {
        public GetCoursesListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
        }
    }
}
