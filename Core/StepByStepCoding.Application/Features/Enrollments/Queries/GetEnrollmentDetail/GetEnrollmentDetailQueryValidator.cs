using FluentValidation;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentDetail
{
    public sealed class GetEnrollmentDetailQueryValidator : AbstractValidator<GetEnrollmentDetailQuery>
    {
        public GetEnrollmentDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
