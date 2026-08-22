using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Multitool.Tests.Shared.Assertions;

internal static class ControllerAssertions
{
    public static void Ok<T>(IActionResult result, T expected)
    {
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    public static void Created<T>(IActionResult result, T expected)
    {
        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Value.Should().BeEquivalentTo(expected);
    }

    public static void NoContent(IActionResult result)
    {
        result.Should().BeOfType<NoContentResult>();
    }

    public static void BadRequest(IActionResult result)
    {
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    public static void NotFound(IActionResult result)
    {
        result.Should().BeOfType<NotFoundResult>();
    }
}
