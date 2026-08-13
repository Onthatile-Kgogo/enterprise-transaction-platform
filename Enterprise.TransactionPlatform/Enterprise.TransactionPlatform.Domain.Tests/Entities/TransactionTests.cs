using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.Exceptions;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Domain.Tests.Entities
{
    public class TransactionTests
    {
        [Fact]
        public void Create_WithValidDetails_ShouldCreateReceivedTransaction()
        {
            // Arrange
            var reference = TransactionReference.Create("TXN-001");
            var currency = Currency.Create("ZAR");
            var money = Money.Create(1500.00m, currency);

            // Act
            var transaction = Transaction.Create(
                reference,
                money,
                TransactionType.Payment,
                "Test payment");

            // Assert
            Assert.NotEqual(Guid.Empty, transaction.TransactionId);
            Assert.Equal(reference, transaction.Reference);
            Assert.Equal(money, transaction.Money);
            Assert.Equal(TransactionType.Payment, transaction.Type);
            Assert.Equal(TransactionStatus.Received, transaction.Status);
            Assert.Equal("Test payment", transaction.Description);
            Assert.NotEqual(default, transaction.CreatedAtUtc);
            Assert.Null(transaction.UpdatedAtUtc);
        }
        [Fact]
        public void MarkPending_WhenReceived_ShouldChangeStatusToPending()
        {
            // Arrange
            var transaction = CreateTransaction();

            // Act
            transaction.MarkPending();

            // Assert
            Assert.Equal(TransactionStatus.Pending, transaction.Status);
            Assert.NotNull(transaction.UpdatedAtUtc);
        }

        [Fact]
        public void StartProcessing_WhenPending_ShouldChangeStatusToProcessing()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.MarkPending();

            // Act
            transaction.StartProcessing();

            // Assert
            Assert.Equal(TransactionStatus.Processing, transaction.Status);
        }

        [Fact]
        public void Complete_WhenProcessing_ShouldChangeStatusToCompleted()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.MarkPending();
            transaction.StartProcessing();

            // Act
            transaction.Complete();

            // Assert
            Assert.Equal(TransactionStatus.Completed, transaction.Status);
        }

        [Fact]
        public void Complete_WhenReceived_ShouldThrowDomainException()
        {
            // Arrange
            var transaction = CreateTransaction();

            // Act
            var action = () => transaction.Complete();

            // Assert
            var exception = Assert.Throws<DomainException>(action);

            Assert.Contains(
                TransactionStatus.Processing.ToString(),
                exception.Message);
        }

        [Fact]
        public void Fail_WhenProcessing_ShouldChangeStatusToFailed()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.MarkPending();
            transaction.StartProcessing();

            // Act
            transaction.Fail();

            // Assert
            Assert.Equal(TransactionStatus.Failed, transaction.Status);
        }

        [Fact]
        public void Fail_WhenCompleted_ShouldThrowDomainException()
        {
            // Arrange
            var transaction = CreateTransaction();
            transaction.MarkPending();
            transaction.StartProcessing();
            transaction.Complete();

            // Act
            var action = () => transaction.Fail();

            // Assert
            Assert.Throws<DomainException>(action);
        }

        private static Transaction CreateTransaction()
        {
            var reference = TransactionReference.Create("TXN-TEST-001");
            var currency = Currency.Create("ZAR");
            var money = Money.Create(100.00m, currency);

            return Transaction.Create(
                reference,
                money,
                TransactionType.Payment,
                "Domain test transaction");
        }

    }
}
