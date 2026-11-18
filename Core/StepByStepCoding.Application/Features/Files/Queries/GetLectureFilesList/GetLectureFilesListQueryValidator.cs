using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFilesList
{
    public sealed class GetLectureFilesListQueryValidator : AbstractValidator<GetLectureFilesListQuery>
    {
        public GetLectureFilesListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
            RuleFor(request => request.LectureId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        }
    }
}
