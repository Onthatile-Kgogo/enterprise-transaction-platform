using Enterprise.TransactionPlatform.Api.Contracts.Transactions;
using Enterprise.TransactionPlatform.Api.Tests.Infrastructure;
using Enterprise.TransactionPlatform.Application.Common.Results;
using Enterprise.TransactionPlatform.Application.Transactions.Submit;
using Enterprise.TransactionPlatform.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace Enterprise.TransactionPlatform.Api.Tests.Controllers.Transactions
{
    public sealed class TransactionsControllerTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;

        public TransactionsControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public async Task GetByIdAsync_WhenTransactionDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            using var client = _fixture.CreateClient();
            var transactionId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/transactions/{transactionId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTransactionIdIsInvalid_ReturnsNotFound()
        {
            // Arrange
            using var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/transactions/not-a-guid");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateStatusAsync_WhenTransactionDoesNotExist_ReturnsNotFoundProblemDetails()
        {
            // Arrange
            using var client = _fixture.CreateClient();
            var transactionId = Guid.NewGuid();
            var request = new UpdateTransactionStatusRequest(TransactionStatus.Pending);

            // Act
            var response = await client.PatchAsJsonAsync($"/api/transactions/{transactionId}/status", request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(HttpStatusCode.NotFound, (HttpStatusCode)problemDetails.Status);
            Assert.Equal("Resource not found", problemDetails.Title);
            Assert.False(string.IsNullOrWhiteSpace(problemDetails.Extensions["traceId"]?.ToString()));
        }

        [Fact]
        public async Task SearchAsync_WhenPageSizeExceedsMaximum_ReturnsBadRequest()
        {
            // Arrange
            using var client = _fixture.CreateClient();

            // Act
            var response = await client.GetAsync("/api/transactions/search?pageSize=101");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<ApplicationError>();

            Assert.NotNull(error);
            Assert.Equal("transaction_search.validation_failed", error.Code);
            Assert.Equal("Page size cannot exceed 100.", error.Message);
        }

        [Fact]
        public async Task SubmitAsync_WhenAmountIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            using var client = _fixture.CreateClient();

            var request = new SubmitTransactionCommand(
                Reference: "TEST-REF-001",
                Amount: -10m,
                Currency: "ZAR",
                Type: TransactionType.Payment,
                Description: "API hardening test");

            // Act
            var response = await client.PostAsJsonAsync("/api/transactions", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<ApplicationError>();

            Assert.NotNull(error);
            Assert.Equal("TRANSACTION.INVALID_AMOUNT", error.Code);
        }

        [Fact]
        public async Task SubmitAsync_WhenRequestIsValid_ReturnsCreated()
        {
            // Arrange
            using var client = _fixture.CreateClient();

            var request = new SubmitTransactionCommand(
                Reference: $"TEST-{Guid.NewGuid():N}",
                Amount: 100m,
                Currency: "ZAR",
                Type: TransactionType.Payment,
                Description: "API hardening test");

            // Act
            var response = await client.PostAsJsonAsync("/api/transactions", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }
    }
}
