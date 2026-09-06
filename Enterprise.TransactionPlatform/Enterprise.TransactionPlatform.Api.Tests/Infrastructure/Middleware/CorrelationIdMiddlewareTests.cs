using Enterprise.TransactionPlatform.Api.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Enterprise.TransactionPlatform.Api.Tests.Infrastructure.Middleware
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdIsNotProvided_ShouldGenerateCorrelationId()
        {
            // Arrange
            var context = CreateHttpContext();
            var middleware = CreateMiddleware();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId =
                context.Items[CorrelationIdMiddleware.ItemKey]?.ToString();

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdIsProvided_ShouldPreserveCorrelationId()
        {
            // Arrange
            const string correlationId = "test-correlation-id";

            var context = CreateHttpContext();
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] =
                correlationId;

            var middleware = CreateMiddleware();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(
                correlationId,
                context.Items[CorrelationIdMiddleware.ItemKey]?.ToString());
        }

        [Fact]
        public async Task InvokeAsync_ShouldAddCorrelationIdToResponseHeader()
        {
            // Arrange
            var context = CreateHttpContext();
            var middleware = CreateMiddleware();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(
                context.Response.Headers.ContainsKey(
                    CorrelationIdMiddleware.HeaderName));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    context.Response.Headers[
                        CorrelationIdMiddleware.HeaderName].ToString()));
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdIsProvided_ShouldReturnSameCorrelationIdInResponseHeader()
        {
            // Arrange
            const string correlationId = "test-correlation-id";

            var context = CreateHttpContext();

            context.Request.Headers[CorrelationIdMiddleware.HeaderName] =
                correlationId;

            var middleware = CreateMiddleware();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(
                correlationId,
                context.Response.Headers[
                    CorrelationIdMiddleware.HeaderName].ToString());
        }

        [Fact]
        public async Task InvokeAsync_WhenCorrelationIdContainsWhitespace_ShouldTrimCorrelationId()
        {
            // Arrange
            const string correlationId = "  test-correlation-id  ";

            var context = CreateHttpContext();

            context.Request.Headers[CorrelationIdMiddleware.HeaderName] =
                correlationId;

            var middleware = CreateMiddleware();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(
                "test-correlation-id",
                context.Items[CorrelationIdMiddleware.ItemKey]?.ToString());

            Assert.Equal(
                "test-correlation-id",
                context.Response.Headers[
                    CorrelationIdMiddleware.HeaderName].ToString());
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNextMiddleware()
        {
            // Arrange
            var nextCalled = false;

            RequestDelegate next = _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = CreateMiddleware(next);
            var context = CreateHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextMiddlewareThrows_ShouldRethrowException()
        {
            // Arrange
            var expectedException = new InvalidOperationException(
                "Test exception.");

            RequestDelegate next = _ =>
                throw expectedException;

            var middleware = CreateMiddleware(next);
            var context = CreateHttpContext();

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => middleware.InvokeAsync(context));

            // Assert
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextMiddlewareThrows_ShouldStillSetCorrelationId()
        {
            // Arrange
            RequestDelegate next = _ =>
                throw new InvalidOperationException("Test exception.");

            var middleware = CreateMiddleware(next);
            var context = CreateHttpContext();

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => middleware.InvokeAsync(context));

            // Assert
            var correlationId =
                context.Items[CorrelationIdMiddleware.ItemKey]?.ToString();

            Assert.False(string.IsNullOrWhiteSpace(correlationId));

            Assert.Equal(
                correlationId,
                context.Response.Headers[
                    CorrelationIdMiddleware.HeaderName].ToString());
        }

        private static CorrelationIdMiddleware CreateMiddleware(
            RequestDelegate? next = null)
        {
            next ??= _ => Task.CompletedTask;

            return new CorrelationIdMiddleware(
                next,
                NullLogger<CorrelationIdMiddleware>.Instance);
        }

        private static DefaultHttpContext CreateHttpContext()
        {
            return new DefaultHttpContext();
        }
    }
}