using FluentValidation;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLectureDetail
{
    public sealed class GetLectureDetailQueryValidator : AbstractValidator<GetLectureDetailQuery>
    {
        public GetLectureDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
