using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFileId
{
    public sealed class GetLectureFileIdQueryValidator : AbstractValidator<GetLectureFileIdQuery>
    {
        public GetLectureFileIdQueryValidator()
        {
            RuleFor(request => request.LectureId).NotEmpty();
        }
    }
}
