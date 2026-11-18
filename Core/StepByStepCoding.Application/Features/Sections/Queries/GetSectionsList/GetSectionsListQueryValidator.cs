using FluentValidation;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionsList
{
    public sealed class GetSectionsListQueryValidator : AbstractValidator<GetSectionsListQuery>
    {
        public GetSectionsListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
            RuleFor(request => request.CourseId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        }
    }
}
