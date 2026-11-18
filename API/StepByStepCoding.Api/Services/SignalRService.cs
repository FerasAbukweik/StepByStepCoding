using Microsoft.AspNetCore.SignalR;
using StepByStepCoding.Application.Contracts.Realtime;
using StepByStepCoding.Application.Models.Realtime;
using StepByStepCoding.Hubs;

namespace StepByStepCoding.Services
{
    public sealed class SignalRService : ISignalRService
    {
        private readonly IHubContext<AppHub, IAppClient> hub;

        public SignalRService(IHubContext<AppHub, IAppClient> hub)
        {
            this.hub = hub;
        }

        public Task ReceiveNotification(Guid userId, NotificationDTO notification)
        {
            return hub.Clients.User(userId.ToString("D")).ReceiveNotification(notification);
        }

        public Task UpdateCode(CodeChangesDTO changes)
        {
            return hub.Clients.Group(GetFileGroupName(changes.FileId)).UpdateCode(changes);
        }

        public Task FileDeleted(Guid lectureId, Guid fileId)
        {
            return hub.Clients.Group(GetFileGroupName(fileId)).FileDeleted(lectureId, fileId);
        }

        public Task LectureEnded(Guid lectureId, Guid fileId)
        {
            return hub.Clients.Group(GetFileGroupName(fileId)).LectureEnded(lectureId);
        }

        private string GetFileGroupName(Guid id) => $"file:{id:N}";


    }
}
