using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.Exceptions;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Domain.Tests.Entities
{
    public class TransactionStatusTests
    {
        private static Transaction CreateTransaction()
        {
            var reference = TransactionReference.Create("TXN-STATUS-001");
            var currency = Currency.Create("ZAR");
            var money = Money.Create(100m, currency);

            return Transaction.Create(reference, money, TransactionType.Payment, "Status transition test");
        }

        [Fact]
        public void Create_ShouldSetStatusToReceived()
        {
            var transaction = CreateTransaction();

            Assert.Equal(TransactionStatus.Received, transaction.Status);
        }

        [Fact]
        public void MarkPending_WhenReceived_ShouldSetStatusToPending()
        {
            var transaction = CreateTransaction();

            transaction.MarkPending();

            Assert.Equal(TransactionStatus.Pending, transaction.Status);
            Assert.NotNull(transaction.UpdatedAtUtc);
        }

        [Fact]
        public void StartProcessing_WhenPending_ShouldSetStatusToProcessing()
        {
            var transaction = CreateTransaction();
            transaction.MarkPending();

            transaction.StartProcessing();

            Assert.Equal(TransactionStatus.Processing, transaction.Status);
        }

        [Fact]
        public void Complete_WhenProcessing_ShouldSetStatusToCompleted()
        {
            var transaction = CreateTransaction();
            transaction.MarkPending();
            transaction.StartProcessing();

            transaction.Complete();

            Assert.Equal(TransactionStatus.Completed, transaction.Status);
        }

        [Fact]
        public void Complete_WhenReceived_ShouldThrowDomainException()
        {
            var transaction = CreateTransaction();

            Assert.Throws<DomainException>(() => transaction.Complete());
        }

        [Fact]
        public void Fail_WhenReceived_ShouldSetStatusToFailed()
        {
            var transaction = CreateTransaction();

            transaction.Fail();

            Assert.Equal(TransactionStatus.Failed, transaction.Status);
        }

        [Fact]
        public void Fail_WhenCompleted_ShouldThrowDomainException()
        {
            var transaction = CreateTransaction();
            transaction.MarkPending();
            transaction.StartProcessing();
            transaction.Complete();

            Assert.Throws<DomainException>(() => transaction.Fail());
        }

        [Fact]
        public void Fail_WhenAlreadyFailed_ShouldThrowDomainException()
        {
            var transaction = CreateTransaction();
            transaction.Fail();

            Assert.Throws<DomainException>(() => transaction.Fail());
        }
    }
}
