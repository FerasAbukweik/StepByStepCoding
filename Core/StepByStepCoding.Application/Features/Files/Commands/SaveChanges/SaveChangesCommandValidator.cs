using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Commands.SaveChanges
{
    public sealed class SaveChangesCommandValidator : AbstractValidator<SaveChangesCommand>
    {
        public SaveChangesCommandValidator()
        {
            RuleFor(request => request.FileName).NotEmpty().MaximumLength(250).When(request => request.FileName is not null);
            RuleFor(request => request.Language).Must(language => language is null || Enum.IsDefined(language.Value));

            RuleFor(request => request.FileId).NotEmpty();
            RuleFor(request => request.BatchId).NotEmpty();
            RuleFor(request => request.BaseRevision).GreaterThanOrEqualTo(0).LessThan(long.MaxValue);
            RuleFor(request => request.ChangeSets).NotNull().Must((request, sets) => sets is { Count: >= 0 and <= 64 }
                && (sets.Count > 0 || request.FileName is not null || request.Language is not null)
                && sets.All(set => set?.Changes is { Count: > 0 and <= 256 }
                    && set.Changes.All(edit => edit is not null && edit.RangeOffset >= 0 && edit.RangeLength >= 0 && edit.Text is not null))
                && sets.Sum(set => (long)set.Changes.Count) <= 1000
                && sets.Sum(set => set.Changes.Sum(edit => (long)edit.Text.Length)) <= 100000)
                .WithMessage("A batch must contain valid change sets, at most 1000 edits and 100000 inserted characters.");
        }
    }
}
