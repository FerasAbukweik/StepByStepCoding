using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Services
{
    public sealed class UserDirectory : IUserDirectory
    {
        private readonly UserManager<ApplicationUser> users;

        public UserDirectory(UserManager<ApplicationUser> users)
        {
            this.users = users;
        }

        public async Task<IReadOnlyDictionary<Guid, string>> GetNames(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Distinct().ToArray();
            if (ids.Length == 0)
                return new Dictionary<Guid, string>();

            return await users.Users.AsNoTracking().Where(user => ids.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => user.FullName, cancellationToken);
        }

        public async Task<bool> IsInRole(Guid userId, string role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await users.FindByIdAsync(userId.ToString());

            return user is not null && await users.IsInRoleAsync(user, role);
        }
    }
}
