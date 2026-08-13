namespace Enterprise.TransactionPlatform.Domain.ValueObjects
{
    public sealed class Currency
    {
        public string Code { get; }
        private Currency(string code)
        {
            Code = code;
        }

        public static Currency Create(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(
                    "Currency code cannot be empty.",
                    nameof(code));
            }

            var normalizedCode = code.Trim().ToUpperInvariant();

            if (normalizedCode.Length != 3 ||
                !normalizedCode.All(char.IsLetter))
            {
                throw new ArgumentException(
                    "Currency code must contain exactly 3 letters.",
                    nameof(code));
            }

            return new Currency(normalizedCode);
        }

        public override string ToString() => Code;

        public override bool Equals(object? obj)
        {
            return obj is Currency currency &&
                   Code == currency.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public static bool operator ==(Currency? left, Currency? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Currency? left, Currency? right)
        {
            return !Equals(left, right);
        }
    }
}
