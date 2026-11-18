using StepByStepCoding.Application.Contracts.Realtime;
using StepByStepCoding.Application.Models.Realtime;
using StepByStepCoding.Services;

namespace StepByStepCoding.Decorators
{
    public sealed class SignalRServiceLoggingDecorator : ISignalRService
    {
        private readonly ISignalRService inner;
        private readonly ILogger<SignalRServiceLoggingDecorator> logger;

        public SignalRServiceLoggingDecorator(ISignalRService inner, ILogger<SignalRServiceLoggingDecorator> logger)
        {
            this.inner = inner;
            this.logger = logger;
        }

        public Task ReceiveNotification(Guid userId, NotificationDTO notification)
        {
            return LogOperation(() => inner.ReceiveNotification(userId, notification), nameof(SignalRService.ReceiveNotification));
        }

        public Task UpdateCode(CodeChangesDTO changes)
        {
            return LogOperation(() => inner.UpdateCode(changes), nameof(SignalRService.UpdateCode));
        }

        public Task FileDeleted(Guid lectureId, Guid fileId)
        {
            return LogOperation(() => inner.FileDeleted(lectureId, fileId), nameof(SignalRService.FileDeleted));
        }

        public Task LectureEnded(Guid lectureId, Guid fileId)
        {
            return LogOperation(() => inner.LectureEnded(lectureId, fileId), nameof(SignalRService.LectureEnded));
        }

        private async Task LogOperation(Func<Task> operation, string method)
        {
            try
            {
                await operation();
                logger.LogDebug("{Service}.{Method} Published", nameof(SignalRService), method);
            }
            catch (Exception exception)
            {
                // A transport failure must not report an already committed write as failed.
                logger.LogWarning("{Service}.{Method} Notification failed ({ExceptionType}); clients must resynchronize",
                    nameof(SignalRService), method, exception.GetType().Name);
            }
        }
    }
}
