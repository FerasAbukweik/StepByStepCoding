using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Queries.GetFileDetail
{
    public sealed class GetFileDetailQueryValidator : AbstractValidator<GetFileDetailQuery>
    {
        public GetFileDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
