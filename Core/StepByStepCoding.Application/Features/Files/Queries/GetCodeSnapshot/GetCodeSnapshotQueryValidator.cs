using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Queries.GetCodeSnapshot
{
    public sealed class GetCodeSnapshotQueryValidator : AbstractValidator<GetCodeSnapshotQuery>
    {
        public GetCodeSnapshotQueryValidator()
        {
            RuleFor(request => request.LectureId).NotEmpty();
        }
    }
}
