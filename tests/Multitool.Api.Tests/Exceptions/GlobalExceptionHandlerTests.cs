using System.Security.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Multitool.Api.Exceptions;
using Multitool.Domain.Exceptions;

namespace Multitool.Api.Tests.Exceptions;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<IProblemDetailsService> _problemDetailsServiceMock;
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _sut;

    public GlobalExceptionHandlerTests()
    {
        _problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _sut = new GlobalExceptionHandler(_problemDetailsServiceMock.Object, _loggerMock.Object);

        _problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(true);
    }

    private static DefaultHttpContext CreateHttpContext() => new();

    [Theory]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(InvalidCredentialException), StatusCodes.Status401Unauthorized)]
    [InlineData(typeof(NotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(UserAlreadyExistsException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(Exception), StatusCodes.Status500InternalServerError)]
    public async Task TryHandleAsync_SetsCorrectStatusCode(Type exceptionType, int expectedStatus)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "error")!;
        var httpContext = CreateHttpContext();

        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        var httpContext = CreateHttpContext();

        var result = await _sut.TryHandleAsync(httpContext, new Exception(), CancellationToken.None);

        result.Should().BeTrue();
    }
}