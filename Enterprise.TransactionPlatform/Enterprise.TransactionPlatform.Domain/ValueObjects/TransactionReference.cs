namespace Enterprise.TransactionPlatform.Domain.ValueObjects
{
    public sealed class TransactionReference
    {
        public string Value { get; }

        private TransactionReference(string value)
        {
            Value = value;
        }

        public static TransactionReference Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Transaction reference cannot be empty.",
                    nameof(value));

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > 100)
                throw new ArgumentException(
                    "Transaction reference cannot exceed 100 characters.",
                    nameof(value));

            return new TransactionReference(normalizedValue);
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            return obj is TransactionReference reference && Value == reference.Value;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(TransactionReference? left, TransactionReference? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TransactionReference? left, TransactionReference? right)
        {
            return !Equals(left, right);
        }
    }
}
