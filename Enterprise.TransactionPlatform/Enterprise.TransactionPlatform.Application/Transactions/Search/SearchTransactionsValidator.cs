using Enterprise.TransactionPlatform.Domain.Enums;

namespace Enterprise.TransactionPlatform.Application.Transactions.Search
{
    public static class SearchTransactionsValidator
    {
        private const int MaxPageSize = 100;

        public static string? Validate(SearchTransactionsQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (query.PageNumber < 1)
                return "Page number must be greater than zero.";

            if (query.PageSize < 1)
                return "Page size must be greater than zero.";

            if (query.PageSize > MaxPageSize)
                return $"Page size cannot exceed {MaxPageSize}.";

            if (query.FromDateUtc.HasValue && query.ToDateUtc.HasValue && query.FromDateUtc > query.ToDateUtc)
                return "From date cannot be greater than to date.";


            if (!string.IsNullOrWhiteSpace(query.Status) && !Enum.TryParse<TransactionStatus>(query.Status, ignoreCase: true, out _))
                return $"Invalid transaction status '{query.Status}'.";


            if (!string.IsNullOrWhiteSpace(query.Type) && !Enum.TryParse<TransactionType>(query.Type, ignoreCase: true, out _))
                return $"Invalid transaction type '{query.Type}'.";

            return null;
        }
    }
}
