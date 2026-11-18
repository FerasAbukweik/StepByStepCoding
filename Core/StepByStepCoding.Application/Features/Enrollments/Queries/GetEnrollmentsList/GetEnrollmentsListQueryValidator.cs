using FluentValidation;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentsList
{
    public sealed class GetEnrollmentsListQueryValidator : AbstractValidator<GetEnrollmentsListQuery>
    {
        public GetEnrollmentsListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
            RuleFor(request => request.SectionId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        }
    }
}
