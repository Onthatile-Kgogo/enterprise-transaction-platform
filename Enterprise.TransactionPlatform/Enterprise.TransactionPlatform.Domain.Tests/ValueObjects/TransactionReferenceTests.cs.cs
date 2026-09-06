using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Domain.Tests.ValueObjects
{
    public class TransactionReferenceTests
    {
        [Fact]
        public void Create_WithValidReference_ShouldCreateReference()
        {
            var reference = TransactionReference.Create("TXN-001");
            Assert.Equal("TXN-001", reference.Value);
        }

        [Fact]
        public void Create_WithWhitespace_ShouldTrimReference()
        {
            var reference = TransactionReference.Create(" TXN-001 ");
            Assert.Equal("TXN-001", reference.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_WithEmptyReference_ShouldThrowArgumentException(string value)
        {
            Assert.Throws<ArgumentException>(() => TransactionReference.Create(value));
        }

        [Fact]
        public void Create_WithReferenceLongerThan100Characters_ShouldThrowArgumentException()
        {
            var value = new string('A', 101);
            Assert.Throws<ArgumentException>(() => TransactionReference.Create(value));
        }

        [Fact]
        public void TwoReferences_WithSameValue_ShouldBeEqual()
        {
            var first = TransactionReference.Create("TXN-001");
            var second = TransactionReference.Create("TXN-001");

            Assert.Equal(first, second);
        }
    }
}
