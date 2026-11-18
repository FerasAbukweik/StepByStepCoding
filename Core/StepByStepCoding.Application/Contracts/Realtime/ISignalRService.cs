using StepByStepCoding.Application.Models.Realtime;

namespace StepByStepCoding.Application.Contracts.Realtime
{
    public interface ISignalRService
    {
        Task ReceiveNotification(Guid userId, NotificationDTO notification);
        Task UpdateCode(CodeChangesDTO changes);
        Task FileDeleted(Guid lectureId, Guid fileId);
        Task LectureEnded(Guid lectureId, Guid fileId);
    }
}
