namespace Enterprise.TransactionPlatform.Domain.ValueObjects
{
    public sealed class Money
    {
        public decimal Amount { get; }
        public Currency Currency { get; }
        private Money(decimal amount, Currency currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, Currency currency)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Transaction amount must be greater than zero.");
            }

            ArgumentNullException.ThrowIfNull(currency);

            return new Money(amount, currency);
        }

        public override string ToString()
        {
            return $"{Amount:0.00} {Currency.Code}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Money money &&
                   Amount == money.Amount &&
                   Currency == money.Currency;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public static bool operator ==(Money? left, Money? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Money? left, Money? right)
        {
            return !Equals(left, right);
        }
    }
}
