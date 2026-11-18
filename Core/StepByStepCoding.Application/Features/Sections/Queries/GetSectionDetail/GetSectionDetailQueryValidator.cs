using FluentValidation;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionDetail
{
    public sealed class GetSectionDetailQueryValidator : AbstractValidator<GetSectionDetailQuery>
    {
        public GetSectionDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
