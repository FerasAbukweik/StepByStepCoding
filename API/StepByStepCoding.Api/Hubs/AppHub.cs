using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Features.Files.Commands.SaveChanges;
using StepByStepCoding.Application.Utilities;
using StepByStepCoding.DTOs.Files;

namespace StepByStepCoding.Hubs
{
    [Authorize]
    public sealed class AppHub : Hub<IAppClient>
    {
        public const string Route = "/hubs/app";

        private readonly IMediator mediator;
        private readonly IAcademicAccess access;
        private readonly IFileRepository files;
        private readonly IConfiguration configuration;

        public AppHub(IMediator mediator, IConfiguration configuration, IAcademicAccess access, IFileRepository files)
        {
            this.mediator = mediator;
            this.access = access;
            this.files = files;
            this.configuration = configuration;
        }

        public override async Task OnConnectedAsync()
        {
            if (!Guid.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out _))
            {
                Context.Abort();
                throw new HubException("A valid authenticated user is required.");
            }

            var origin = Context.GetHttpContext()?.Request.Headers.Origin.ToString();
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            if (!string.IsNullOrEmpty(origin)
                && !origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                Context.Abort();
                throw new HubException("This origin is not allowed.");
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinFile(Guid fileId)
        {
            var userId = access.CurrentScope().UserId;
            var file = await files.GetWithEnrollment(fileId, userId, Context.ConnectionAborted)
                ?? throw new NotFoundException("File was not found.");
            access.ReadFile(file);
            await Groups.AddToGroupAsync(Context.ConnectionId, GetFileGroupName(fileId), Context.ConnectionAborted);
        }

        public Task LeaveFile(Guid fileId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetFileGroupName(fileId), Context.ConnectionAborted);
        }

        public async Task SaveChanges(Guid fileId, SaveChangesDTO request)
        {
            if (request is null)
                throw new HubException("A change batch is required.");

            var changes = await mediator.Send(new SaveChangesCommand
            {
                FileId = fileId,
                FileName = request.FileName,
                Language = request.Language,
                BatchId = request.BatchId,
                BaseRevision = request.BaseRevision,
                ChangeSets = request.ChangeSets
            }, Context.ConnectionAborted);

            await Groups.AddToGroupAsync(Context.ConnectionId, GetFileGroupName(fileId), Context.ConnectionAborted);
            // Include the sender so all sessions receive the committed revision.
            await Clients.Group(GetFileGroupName(fileId)).UpdateCode(changes);
        }

        private string GetFileGroupName(Guid id) => $"file:{id:N}";
    }
}
