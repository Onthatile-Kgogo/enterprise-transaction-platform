using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Enterprise.TransactionPlatform.Api.Tests.Health
{
    public sealed class HealthEndpointTests
    {
        [Fact]
        public async Task LiveEndpoint_WhenApplicationIsRunning_ShouldReturnHealthy()
        {
            // Arrange
            await using var factory =
                new WebApplicationFactory<Enterprise.TransactionPlatform.Api.Program>();

            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/live");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("Healthy", content);
        }

        [Fact]
        public async Task ReadyEndpoint_WhenDatabaseIsAvailable_ShouldReturnHealthy()
        {
            // Arrange
            await using var factory =
                new WebApplicationFactory<Enterprise.TransactionPlatform.Api.Program>();

            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/ready");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("Healthy", content);
        }

        [Fact]
        public async Task ReadyEndpoint_WhenDatabaseIsUnavailable_ShouldReturnServiceUnavailable()
        {
            // Arrange
            await using var factory =
                new WebApplicationFactory<Enterprise.TransactionPlatform.Api.Program>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.ConfigureAppConfiguration(
                            (_, configuration) =>
                            {
                                configuration.AddInMemoryCollection(
                                    new Dictionary<string, string?>
                                    {
                                        ["ConnectionStrings:EnterpriseTransactionPlatform"] =
                                            "Server=invalid-server;" +
                                            "Database=InvalidDatabase;" +
                                            "User Id=invalid;" +
                                            "Password=invalid;"
                                    });
                            });
                    });

            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/ready");

            // Assert
            Assert.Equal(
                System.Net.HttpStatusCode.ServiceUnavailable,
                response.StatusCode);
        }
    }
}