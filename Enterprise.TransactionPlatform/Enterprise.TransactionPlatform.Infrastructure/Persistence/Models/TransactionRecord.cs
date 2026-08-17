namespace Enterprise.TransactionPlatform.Infrastructure.Persistence.Models
{
    internal sealed class TransactionRecord
    {
        public Guid TransactionId { get; init; }
        public string Reference { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
