using FluentValidation;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamsList
{
    public sealed class GetExamsListQueryValidator : AbstractValidator<GetExamsListQuery>
    {
        public GetExamsListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
            RuleFor(request => request.SectionId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        }
    }
}
