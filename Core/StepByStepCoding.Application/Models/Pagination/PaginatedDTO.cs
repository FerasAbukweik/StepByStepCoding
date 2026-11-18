namespace StepByStepCoding.Application.Models.Pagination
{
    public sealed class PaginatedDTO<T>
    {
        public required IReadOnlyList<T> Items
        {
            get; init;
        }

        public required int TotalCount
        {
            get; init;
        }
    }
}
