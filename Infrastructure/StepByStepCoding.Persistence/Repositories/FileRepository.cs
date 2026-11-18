using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.EntityFrameworkCore.Repositories;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class FileRepository : Repository<CodeFile, ApplicationDbContext>, IFileRepository
    {
        public FileRepository(ApplicationDbContext context) : base(context) { }

        public Task<CodeFile?> GetWithEnrollment(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return context.Files.AsNoTracking()
                .Include(entity => ((LectureFile)entity).Lecture!.Section!.Enrollments
                    .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active))
                .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }
    }
}
