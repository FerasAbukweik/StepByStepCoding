namespace StepByStepCoding.Domain.Common;

public abstract class Auditable
{
    public Guid? CreatedBy
    {
        get; set;
    }

    public DateTime? CreationTime
    {
        get; set;
    }

    public Guid? LastModifiedBy
    {
        get; set;
    }

    public DateTime? LastModifiedDate
    {
        get; set;
    }
}
