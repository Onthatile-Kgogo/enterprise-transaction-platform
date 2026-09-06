namespace Enterprise.TransactionPlatform.Application.Common
{
    public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalRecords);
}
