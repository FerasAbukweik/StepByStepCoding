using FluentValidation;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCourseDetail
{
    public sealed class GetCourseDetailQueryValidator : AbstractValidator<GetCourseDetailQuery>
    {
        public GetCourseDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
