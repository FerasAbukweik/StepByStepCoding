using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFileByLecture
{
    public sealed class GetLectureFileByLectureQueryValidator : AbstractValidator<GetLectureFileByLectureQuery>
    {
        public GetLectureFileByLectureQueryValidator()
        {
            RuleFor(request => request.LectureId).NotEmpty();
        }
    }
}
