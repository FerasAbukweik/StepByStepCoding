using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Lectures.Commands.DeleteLecture
{
    public sealed class DeleteLectureCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public override string Key() => $"{CacheGroups.File}:";

        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
        {
            get; set;
        }
    }
}
