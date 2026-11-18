using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Persistence.Repositories;
using StepByStepCoding.EntityFrameworkCore.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StepByStepCoding.Persistence.ExtensionMethods
{
    public static class PersistenceServicesExtensionMethods
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("ApplicationConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:ApplicationConnection is required."),
                sql => sql.EnableRetryOnFailure()));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddEntityFrameworkServices();
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IUnitOfWork<ApplicationDbContext>>());

            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<ISectionRepository, SectionRepository>();
            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            services.AddScoped<ILectureRepository, LectureRepository>();
            services.AddScoped<ILectureFileRepository, LectureFileRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IWorkspaceFileRepository, WorkspaceFileRepository>();
            services.AddScoped<IExamRepository, ExamRepository>();
            services.AddScoped<IExamQuestionRepository, ExamQuestionRepository>();
            services.AddScoped<IExamAttemptRepository, ExamAttemptRepository>();
            services.AddScoped<ISubmissionRepository, SubmissionRepository>();

            return services;
        }
    }
}
