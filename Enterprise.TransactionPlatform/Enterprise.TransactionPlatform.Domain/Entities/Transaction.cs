using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.Exceptions;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Domain.Entities
{
    public sealed class Transaction
    {
        public Guid TransactionId { get; private set; }
        public TransactionReference Reference { get; private set; }
        public Money Money { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        private Transaction(Guid transactionId, TransactionReference reference, Money money, TransactionType type, string? description)
        {
            TransactionId = transactionId;
            Reference = reference;
            Money = money;
            Type = type;
            Description = description;
            Status = TransactionStatus.Received;
            CreatedAtUtc = DateTime.UtcNow;
        }
        public static Transaction Create(TransactionReference reference, Money money, TransactionType type, string? description = null)
        {
            ArgumentNullException.ThrowIfNull(reference);
            ArgumentNullException.ThrowIfNull(money);

            if (!Enum.IsDefined(type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    "Transaction type is invalid.");
            }

            if (!string.IsNullOrWhiteSpace(description) &&
                description.Length > 500)
            {
                throw new ArgumentException(
                    "Transaction description cannot exceed 500 characters.",
                    nameof(description));
            }

            return new Transaction(
                Guid.NewGuid(),
                reference,
                money,
                type,
                description?.Trim());
        }

        public void MarkPending()
        {
            EnsureStatus(TransactionStatus.Received);
            Status = TransactionStatus.Pending;
            Touch();
        }
        public void StartProcessing()
        {
            EnsureStatus(TransactionStatus.Pending);

            Status = TransactionStatus.Processing;
            Touch();
        }
        public void Complete()
        {
            EnsureStatus(TransactionStatus.Processing);

            Status = TransactionStatus.Completed;
            Touch();
        }
        public void Fail()
        {
            if (Status is TransactionStatus.Completed or TransactionStatus.Failed)
            {
                throw new DomainException(
                    $"Transaction cannot fail when its current status is {Status}.");
            }

            Status = TransactionStatus.Failed;
            Touch();
        }

        private void EnsureStatus(TransactionStatus requiredStatus)
        {
            if (Status != requiredStatus)
            {
                throw new DomainException(
                    $"Transaction must be in {requiredStatus} status to perform this operation. Current status is {Status}.");
            }
        }
        private void Touch()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
