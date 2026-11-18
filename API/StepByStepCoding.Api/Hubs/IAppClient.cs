using StepByStepCoding.Application.Models.Realtime;

namespace StepByStepCoding.Hubs
{
    public interface IAppClient
    {
        Task ReceiveNotification(NotificationDTO notification);
        Task UpdateCode(CodeChangesDTO changes);
        Task FileDeleted(Guid lectureId, Guid fileId);
        Task LectureEnded(Guid lectureId);
    }
}
