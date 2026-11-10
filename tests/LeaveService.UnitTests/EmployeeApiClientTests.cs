using System.Net;
using System.Net.Http.Json;
using LeaveService.Services;
using Microsoft.Extensions.Configuration;

namespace LeaveService.UnitTests;

public class EmployeeApiClientTests
{
    private static EmployeeApiClient CreateClient(
        HttpMessageHandler handler,
        TimeSpan? timeout = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
            Timeout = timeout ?? TimeSpan.FromSeconds(100)
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ServiceAuthentication:ApiKey"] = "test-api-key"
                })
            .Build();

        return new EmployeeApiClient(httpClient, configuration);
    }

    [Fact]
    public async Task ExistsAsync_OkResponse_ReturnsTrue()
    {
        var client = CreateClient(
            new FakeEmployeeHandler(HttpStatusCode.OK));

        var result = await client.ExistsAsync(Guid.NewGuid());

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NotFoundResponse_ReturnsFalse()
    {
        var client = CreateClient(
            new FakeEmployeeHandler(HttpStatusCode.NotFound));

        var result = await client.ExistsAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_ServerError_ThrowsHttpRequestException()
    {
        var client = CreateClient(
            new FakeEmployeeHandler(
                HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.ExistsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsAsync_ConnectionFailure_ThrowsHttpRequestException()
    {
        var client = CreateClient(
            new ConnectionFailureHandler());

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.ExistsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsAsync_Timeout_ThrowsTaskCanceledException()
    {
        var client = CreateClient(
            new TimeoutHandler(),
            TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.ExistsAsync(Guid.NewGuid()));
    }

    private sealed class FakeEmployeeHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public FakeEmployeeHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode);

            if (_statusCode == HttpStatusCode.OK)
            {
                var employee = new EmployeeInfo(
                    Guid.NewGuid(),
                    "Test Employee",
                    "test.employee@example.com",
                    "Engineering",
                    "Employee",
                    DateTime.UtcNow);

                response.Content = JsonContent.Create(employee);
            }

            return Task.FromResult(response);
        }
    }

    private sealed class ConnectionFailureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException(
                "Simulated connection failure.");
        }
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(
                Timeout.Infinite,
                cancellationToken);

            throw new InvalidOperationException(
                "Unreachable code.");
        }
    }
}