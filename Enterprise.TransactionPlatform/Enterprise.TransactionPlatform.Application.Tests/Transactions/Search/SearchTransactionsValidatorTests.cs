using Enterprise.TransactionPlatform.Application.Transactions.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.TransactionPlatform.Application.Tests.Transactions.Search
{
    public sealed class SearchTransactionsValidatorTests
    {
        [Fact]
        public void Validate_WithValidQuery_ShouldReturnNull()
        {
            // Arrange
            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null);

            // Act
            var result = SearchTransactionsValidator.Validate(query);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Validate_WithPageNumberLessThanOne_ShouldReturnError()
        {
            // Arrange
            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageNumber: 0);

            // Act
            var result = SearchTransactionsValidator.Validate(query);

            // Assert
            Assert.Equal(
                "Page number must be greater than zero.",
                result);
        }

        [Fact]
        public void Validate_WithPageSizeGreaterThanMaximum_ShouldReturnError()
        {
            // Arrange
            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null,
                PageSize: 101);

            // Act
            var result = SearchTransactionsValidator.Validate(query);

            // Assert
            Assert.Equal(
                "Page size cannot exceed 100.",
                result);
        }

        [Fact]
        public void Validate_WithFromDateGreaterThanToDate_ShouldReturnError()
        {
            // Arrange
            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: null,
                Currency: null,
                FromDateUtc: new DateTime(2026, 8, 18),
                ToDateUtc: new DateTime(2026, 8, 17));

            // Act
            var result = SearchTransactionsValidator.Validate(query);

            // Assert
            Assert.Equal(
                "From date cannot be greater than to date.",
                result);
        }

        [Fact]
        public void Validate_WithInvalidStatus_ShouldReturnError()
        {
            // Arrange
            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: "SomethingInvalid",
                Type: null,
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null);

            // Act
            var result = SearchTransactionsValidator.Validate(query);

            // Assert
            Assert.Equal(
                "Invalid transaction status 'SomethingInvalid'.",
                result);
        }

        [Fact]
        public void Validate_WithInvalidType_ShouldReturnError()
        {
            // Arrange
            var query = new SearchTransactionsQuery(
                Reference: null,
                Status: null,
                Type: "SomethingInvalid",
                Currency: null,
                FromDateUtc: null,
                ToDateUtc: null);

            // Act
            var result = SearchTransactionsValidator.Validate(query);

            // Assert
            Assert.Equal(
                "Invalid transaction type 'SomethingInvalid'.",
                result);
        }
    }
}
