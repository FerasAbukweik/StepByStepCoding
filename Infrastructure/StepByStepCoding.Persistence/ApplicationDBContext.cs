using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Domain.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IUserService? userService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserService? userService = null)
            : base(options)
        {
            this.userService = userService;
        }

        public DbSet<Course> Courses => Set<Course>();

        public DbSet<Section> Sections => Set<Section>();

        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        public DbSet<Lecture> Lectures => Set<Lecture>();

        public DbSet<CodeFile> Files => Set<CodeFile>();

        public DbSet<WorkspaceFile> WorkspaceFiles => Set<WorkspaceFile>();

        public DbSet<LectureFile> LectureFiles => Set<LectureFile>();

        public DbSet<Exam> Exams => Set<Exam>();

        public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();

        public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();

        public DbSet<Submission> Submissions => Set<Submission>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            var utcDateTimeConverter = new ValueConverter<DateTime, DateTime>(
                value => value.ToUniversalTime(),
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if ((Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == typeof(DateTime))
                        property.SetValueConverter(utcDateTimeConverter);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AcademicEntity>().Where(entry => entry.State == EntityState.Modified))
            {
                entry.Property(entity => entity.Version).OriginalValue = entry.Entity.OriginalVersion == Guid.Empty
                    ? entry.Entity.Version : entry.Entity.OriginalVersion;
            }

            if (userService is not null)
            {
                foreach (var entry in ChangeTracker.Entries<Auditable>())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.CreationTime = DateTime.UtcNow;
                            entry.Entity.CreatedBy = userService.GetUserId();
                            break;

                        case EntityState.Modified:
                            entry.Entity.LastModifiedDate = DateTime.UtcNow;
                            entry.Entity.LastModifiedBy = userService.GetUserId();
                            break;
                    }
                }
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The record changed. Reload it before trying again.");
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException sql
                && sql.Number is 2601 or 2627 or 547)
            {
                throw new ConflictException("The operation conflicts with an existing or related record.");
            }
        }
    }
}
