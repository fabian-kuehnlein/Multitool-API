using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Multitool.Infrastructure.ApiClients;
using Multitool.Tests.Shared.Assertions;

namespace Multitool.Infrastructure.Tests;

public class CalendarApiClientTests
{
    private readonly Mock<ILogger<CalendarApiClient>> _loggerMock = new();

    private static HttpClient CreateHttpClient(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new StubHttpMessageHandler(jsonResponse, statusCode);
        return new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };
    }

    private static string WrapResponse(string feiertageArray) =>
        $"{{ \"status\": \"ok\", \"feiertage\": {feiertageArray} }}";

    // GetHolidaysAsync

    [Fact]
    public async Task GetHolidaysAsync_WhenResponseIsValid_ReturnsHolidays()
    {
        // Arrange
        var json = WrapResponse("""[{ "date": "2026-01-01", "fname": "Neujahrstag" }, { "date": "2026-12-25", "fname": "1. Weihnachtstag" }]""");
        var client = CreateHttpClient(json);
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act
        var result = await sut.GetHolidaysAsync("2026");

        // Assert
        AssertEx.AreEqual(2, result.Count);
        AssertEx.AreEqual("Neujahrstag", result[0].Name);
        AssertEx.AreEqual(new DateTime(2026, 1, 1), result[0].Date);
        AssertEx.AreEqual("1. Weihnachtstag", result[1].Name);
        AssertEx.AreEqual(new DateTime(2026, 12, 25), result[1].Date);
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenFeiertageIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var json = WrapResponse("[]");
        var client = CreateHttpClient(json);
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act
        var result = await sut.GetHolidaysAsync("2026");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenFeiertageIsNull_ReturnsEmptyList()
    {
        // Arrange
        var json = """{ "status": "ok", "feiertage": null }""";
        var client = CreateHttpClient(json);
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act
        var result = await sut.GetHolidaysAsync("2026");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenResponseIsSuccessButNoFeiertageKey_ReturnsEmptyList()
    {
        // Arrange
        var json = """{ "status": "ok" }""";
        var client = CreateHttpClient(json);
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act
        var result = await sut.GetHolidaysAsync("2026");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenStatusCodeIsError_ThrowsHttpRequestException()
    {
        // Arrange
        var client = CreateHttpClient("", HttpStatusCode.InternalServerError);
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act & Assert
        await AssertEx.Throws<HttpRequestException>(() => sut.GetHolidaysAsync("2026"));
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenResponseIsInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var client = CreateHttpClient("not-json");
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act & Assert
        await AssertEx.Throws<JsonException>(() => sut.GetHolidaysAsync("2026"));
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenCalled_SendsCorrectUrl()
    {
        // Arrange
        string? capturedQuery = null;
        var handler = new CallbackHttpMessageHandler(WrapResponse("[]"), req =>
        {
            capturedQuery = req.RequestUri!.Query;
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act
        await sut.GetHolidaysAsync("2025");

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Contains("years=2025", capturedQuery);
        Assert.Contains("states=by", capturedQuery);
    }

    [Fact]
    public async Task GetHolidaysAsync_WhenSingleHoliday_ReturnsCorrectMapping()
    {
        // Arrange
        var json = WrapResponse("""[{ "date": "2026-03-08", "fname": "Internationaler Frauentag" }]""");
        var client = CreateHttpClient(json);
        var sut = new CalendarApiClient(client, _loggerMock.Object);

        // Act
        var result = await sut.GetHolidaysAsync("2026");

        // Assert
        Assert.Single(result);
        AssertEx.AreEqual("Internationaler Frauentag", result[0].Name);
        AssertEx.AreEqual(new DateTime(2026, 3, 8), result[0].Date);
    }

    private class StubHttpMessageHandler(string responseBody, HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseBody)
            });
        }
    }

    private class CallbackHttpMessageHandler(string responseBody, Action<HttpRequestMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            callback(request);
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseBody)
            });
        }
    }
}
