using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using StepByStepCoding.Security.Contracts.Repositories;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Repositories
{
    public sealed class UserLookupRepository : Repository<ApplicationUser, ApplicationSecurityDbContext>, IUserLookupRepository
    {
        public UserLookupRepository(ApplicationSecurityDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<UserLookupDTO>> Search(string role, string? search, CancellationToken cancellationToken)
        {
            var query = context.Users.AsNoTracking().Where(user => context.UserRoles.Any(membership =>
                membership.UserId == user.Id && context.Roles.Any(item => item.Id == membership.RoleId && item.Name == role)));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(user => user.FullName.Contains(search) || user.Email!.Contains(search));

            return await query.OrderBy(user => user.FullName).ThenBy(user => user.Id).Take(20)
                .Select(user => new UserLookupDTO { Id = user.Id, FullName = user.FullName, Email = user.Email! })
                .ToListAsync(cancellationToken);
        }
    }
}
