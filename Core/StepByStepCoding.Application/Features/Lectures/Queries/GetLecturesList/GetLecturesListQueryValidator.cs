using FluentValidation;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLecturesList
{
    public sealed class GetLecturesListQueryValidator : AbstractValidator<GetLecturesListQuery>
    {
        public GetLecturesListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
            RuleFor(request => request.SectionId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        }
    }
}
