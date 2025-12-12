using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VoskAPP;
using Xunit;

namespace VoskAPP.Tests;

public class ChatServiceTests
{
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _responseJson;
        private readonly HttpStatusCode _statusCode;

        public FakeHandler(string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseJson = responseJson;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var msg = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseJson)
            };
            return Task.FromResult(msg);
        }
    }

    [Fact]
    public async Task GetChatAsync_ReturnsContent_WhenResponseValid()
    {
        // Arrange
        string json = "{ \"choices\": [ { \"message\": { \"content\": \"[主旨1,主旨2,主旨3]\" } } ] }";
        var handler = new FakeHandler(json);
        var httpClient = new HttpClient(handler);
        var service = new ChatService(httpClient, "test-key");

        // Act
        string? result = await service.GetChatAsync("TWN");

        // Assert
        Assert.Equal("[主旨1,主旨2,主旨3]", result);
    }

    [Fact]
    public async Task GetChatAsync_ReturnsNull_WhenStatusNotSuccess()
    {
        // Arrange
        var handler = new FakeHandler("{}", HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(handler);
        var service = new ChatService(httpClient, "test-key");

        // Act
        string? result = await service.GetChatAsync("USA");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetChatAsync_ReturnsNull_OnMalformedJson()
    {
        // Arrange malformed json
        var handler = new FakeHandler("not-json");
        var httpClient = new HttpClient(handler);
        var service = new ChatService(httpClient, "test-key");

        // Act
        string? result = await service.GetChatAsync("JPN");

        // Assert
        Assert.Null(result);
    }
}
