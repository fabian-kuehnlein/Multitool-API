using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Multitool.Tests.Shared.Assertions;

internal static class ControllerAssertions
{
    /// <summary>
    /// Asserts that the result is an <see cref="OkObjectResult"/>
    /// and that its value is equivalent to the expected object.
    /// </summary>
    public static void Ok<T>(IActionResult result, T expected)
    {
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Asserts that the result is a <see cref="CreatedResult"/> (HTTP 201)
    /// and that its value is equivalent to the expected object.
    /// </summary>
    public static void Created<T>(IActionResult result, T expected)
    {
        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Value.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Asserts that the result is a <see cref="NoContentResult"/> (HTTP 204).
    /// </summary>
    public static void NoContent(IActionResult result)
    {
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Asserts that the result is a <see cref="BadRequestObjectResult"/> (HTTP 400).
    /// </summary>
    public static void BadRequest(IActionResult result)
    {
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Asserts that the result is a <see cref="NotFoundResult"/> (HTTP 404).
    /// </summary>
    public static void NotFound(IActionResult result)
    {
        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Asserts that the result is a <see cref="FileContentResult"/> with the expected
    /// content type, download file name, and file contents.
    /// </summary>
    public static void FileResult(IActionResult result, string contentType, string fileDownloadName, byte[] contents)
    {
        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be(contentType);
        file.FileDownloadName.Should().Be(fileDownloadName);
        file.FileContents.Should().BeEquivalentTo(contents);
    }
}
