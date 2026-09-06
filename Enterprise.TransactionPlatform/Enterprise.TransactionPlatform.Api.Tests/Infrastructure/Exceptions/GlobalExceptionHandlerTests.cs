using Enterprise.TransactionPlatform.Api.Infrastructure.Exceptions;
using Enterprise.TransactionPlatform.Api.Infrastructure.Middleware;
using Enterprise.TransactionPlatform.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Enterprise.TransactionPlatform.Api.Tests.Infrastructure.Exceptions
{
    public class GlobalExceptionHandlerTests
    {
        [Fact]
        public async Task TryHandleAsync_WhenUnexpectedExceptionOccurs_ShouldReturnInternalServerError()
        {
            // Arrange
            var context = CreateHttpContext();
            var exception = new InvalidOperationException(
                "Sensitive internal error.");

            var handler = CreateHandler();

            // Act
            var handled = await handler.TryHandleAsync(
                context,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(handled);
            Assert.Equal(
                StatusCodes.Status500InternalServerError,
                context.Response.StatusCode);
        }

        [Fact]
        public async Task TryHandleAsync_WhenUnexpectedExceptionOccurs_ShouldIncludeTraceId()
        {
            // Arrange
            var context = CreateHttpContext();
            var exception = new InvalidOperationException();
            var handler = CreateHandler();

            // Act
            await handler.TryHandleAsync(
                context,
                exception,
                CancellationToken.None);

            // Assert
            var problemDetails = await ReadProblemDetailsAsync(context);

            Assert.True(
                problemDetails.TryGetValue(
                    "traceId",
                    out var traceId));

            Assert.Equal(
                context.TraceIdentifier,
                traceId?.ToString());
        }

        [Fact]
        public async Task TryHandleAsync_WhenUnexpectedExceptionOccurs_ShouldIncludeCorrelationId()
        {
            // Arrange
            const string correlationId = "test-correlation-id";

            var context = CreateHttpContext();

            context.Items[CorrelationIdMiddleware.ItemKey] =
                correlationId;

            var exception = new InvalidOperationException();
            var handler = CreateHandler();

            // Act
            await handler.TryHandleAsync(
                context,
                exception,
                CancellationToken.None);

            // Assert
            var problemDetails = await ReadProblemDetailsAsync(context);

            Assert.True(
                problemDetails.TryGetValue(
                    "correlationId",
                    out var actualCorrelationId));

            Assert.Equal(
                correlationId,
                actualCorrelationId?.ToString());
        }

        [Fact]
        public async Task TryHandleAsync_WhenUnexpectedExceptionOccurs_ShouldNotExposeExceptionDetails()
        {
            // Arrange
            var context = CreateHttpContext();
            var exception = new InvalidOperationException("Sensitive database information.");
            var handler = CreateHandler();

            // Act
            await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            var problemDetails = await ReadProblemDetailsAsync(context);

            Assert.Equal("An unexpected error occurred", problemDetails["title"]?.ToString());
            Assert.Equal("An unexpected error occurred while processing the request.", problemDetails["detail"]?.ToString());
            Assert.DoesNotContain("Sensitive database information.", problemDetails.ToString());
        }

        [Fact]
        public async Task TryHandleAsync_WhenKeyNotFoundExceptionOccurs_ShouldReturnNotFound()
        {
            // Arrange
            var context = CreateHttpContext();
            var exception = new KeyNotFoundException("Transaction not found.");
            var handler = CreateHandler();

            // Act
            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.True(handled);
            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task TryHandleAsync_WhenDomainExceptionOccurs_ShouldReturnConflict()
        {
            // Arrange
            var context = CreateHttpContext();

            var exception = CreateDomainException();
            var handler = CreateHandler();

            // Act
            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.True(handled);

            Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        }

        [Fact]
        public async Task TryHandleAsync_WhenArgumentExceptionOccurs_ShouldReturnBadRequest()
        {
            // Arrange
            var context = CreateHttpContext();

            var exception = new ArgumentException("Invalid request.");

            var handler = CreateHandler();

            // Act
            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.True(handled);

            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task TryHandleAsync_WhenArgumentOutOfRangeExceptionOccurs_ShouldReturnBadRequest()
        {
            // Arrange
            var context = CreateHttpContext();
            var exception = new ArgumentOutOfRangeException("amount", "Amount is outside the allowed range.");
            var handler = CreateHandler();

            // Act
            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.True(handled);

            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task TryHandleAsync_ShouldReturnProblemJsonContentType()
        {
            // Arrange
            var context = CreateHttpContext();
            var exception = new InvalidOperationException();
            var handler = CreateHandler();

            // Act
            await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.Equal("application/problem+json", context.Response.ContentType);
        }

        private static GlobalExceptionHandler CreateHandler()
        {
            return new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        }
        private static DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext
            {
                Response =
                {
                    Body = new MemoryStream()
                }
            };

            return context;
        }
        private static async Task<Dictionary<string, object?>> ReadProblemDetailsAsync(DefaultHttpContext context)
        {
            context.Response.Body.Position = 0;

            using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
            var json = await reader.ReadToEndAsync();

            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? [];
        }
        private static DomainException CreateDomainException()
        {
            return new TestDomainException("Transaction conflict.");
        }
        private sealed class TestDomainException : DomainException
        {
            public TestDomainException(string message) : base(message)
            {
            }
        }
    }
}