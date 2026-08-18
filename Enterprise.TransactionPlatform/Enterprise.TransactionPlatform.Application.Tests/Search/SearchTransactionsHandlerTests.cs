using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Application.Common;
using Enterprise.TransactionPlatform.Application.Transactions.Search;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.TransactionPlatform.Application.Tests.Search
{
    public sealed class SearchTransactionsHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WithValidQuery_ShouldReturnPagedResult()
        {
            // Arrange
            var transaction = Transaction.Create(
                TransactionReference.Create($"TEST-{Guid.NewGuid():N}"),
                Money.Create(
                    100m,
                    Currency.Create("ZAR")),
                TransactionType.Payment,
                "Search test");

            var repository = new FakeTransactionRepository(
                new[] { transaction },
                totalRecords: 1);

            var currencyProvider = new FakeSupportedCurrencyProvider(true);

            var handler = new SearchTransactionsHandler(
                repository,
                currencyProvider);

            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 1,
                PageSize: 20);

            // Act
            var result = await handler.HandleAsync(query);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Single(result.Value.Items);
            Assert.Equal(1, result.Value.TotalRecords);
            Assert.Equal(1, result.Value.TotalPages);
            Assert.Equal(1, result.Value.PageNumber);
            Assert.Equal(20, result.Value.PageSize);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidPageNumber_ShouldReturnFailure()
        {
            // Arrange
            var repository = new FakeTransactionRepository(
                Array.Empty<Transaction>(),
                totalRecords: 0);

            var currencyProvider = new FakeSupportedCurrencyProvider(true);

            var handler = new SearchTransactionsHandler(
                repository,
                currencyProvider);

            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 0,
                PageSize: 20);

            // Act
            var result = await handler.HandleAsync(query);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);

            Assert.Equal(
                "transaction_search.validation_failed",
                result.Error.Code);
        }

        [Fact]
        public async Task HandleAsync_WithUnsupportedCurrency_ShouldReturnFailure()
        {
            // Arrange
            var repository = new FakeTransactionRepository(
                Array.Empty<Transaction>(),
                totalRecords: 0);

            var currencyProvider = new FakeSupportedCurrencyProvider(false);

            var handler = new SearchTransactionsHandler(
                repository,
                currencyProvider);

            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: "ABC",
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 1,
                PageSize: 20);

            // Act
            var result = await handler.HandleAsync(query);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);

            Assert.Equal(
                "transaction_search.unsupported_currency",
                result.Error.Code);
        }

        [Fact]
        public async Task HandleAsync_WithMultiplePages_ShouldCalculateTotalPages()
        {
            // Arrange
            var repository = new FakeTransactionRepository(
                Array.Empty<Transaction>(),
                totalRecords: 45);

            var currencyProvider = new FakeSupportedCurrencyProvider(true);

            var handler = new SearchTransactionsHandler(
                repository,
                currencyProvider);

            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 1,
                PageSize: 20);

            // Act
            var result = await handler.HandleAsync(query);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(45, result.Value.TotalRecords);
            Assert.Equal(3, result.Value.TotalPages);
        }

        [Fact]
        public async Task HandleAsync_WithZeroRecords_ShouldReturnZeroTotalPages()
        {
            // Arrange
            var repository = new FakeTransactionRepository(
                Array.Empty<Transaction>(),
                totalRecords: 0);

            var currencyProvider = new FakeSupportedCurrencyProvider(true);

            var handler = new SearchTransactionsHandler(
                repository,
                currencyProvider);

            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 1,
                PageSize: 20);

            // Act
            var result = await handler.HandleAsync(query);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value.Items);
            Assert.Equal(0, result.Value.TotalRecords);
            Assert.Equal(0, result.Value.TotalPages);
        }

        private sealed class FakeSupportedCurrencyProvider
            : ISupportedCurrencyProvider
        {
            private readonly bool isSupported;

            public FakeSupportedCurrencyProvider(bool isSupported)
            {
                this.isSupported = isSupported;
            }

            public Task<bool> IsSupportedAsync(
                string currencyCode,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(isSupported);
            }
        }

        private sealed class FakeTransactionRepository : ITransactionRepository
        {
            private readonly IReadOnlyCollection<Transaction> transactions;
            private readonly int totalRecords;

            public FakeTransactionRepository(IReadOnlyCollection<Transaction> transactions, int totalRecords)
            {
                this.transactions = transactions;
                this.totalRecords = totalRecords;
            }

            public Task<PagedResult<Transaction>> SearchAsync(TransactionSearchCriteria criteria, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new PagedResult<Transaction>(transactions, totalRecords));
            }

            public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<Transaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task UpdateStatusAsync(Transaction transaction, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}
