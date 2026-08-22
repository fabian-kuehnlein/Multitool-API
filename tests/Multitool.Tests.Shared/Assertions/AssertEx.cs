using Microsoft.AspNetCore.Mvc;

namespace Multitool.Tests.Shared.Assertions;

public static class AssertEx
{
    // Controller Assertions
    public static void Ok<T>(IActionResult result, T expected)
        => ControllerAssertions.Ok(result, expected);

    public static void Created<T>(IActionResult result, T expected)
        => ControllerAssertions.Created(result, expected);

    public static void NoContent(IActionResult result)
        => ControllerAssertions.NoContent(result);

    public static void BadRequest(IActionResult result)
        => ControllerAssertions.BadRequest(result);

    public static void NotFound(IActionResult result)
        => ControllerAssertions.NotFound(result);

    // // Service Assertions
    // public static void Throws<TException>(Func<Task> action)
    //     where TException : Exception
    //     => ServiceAssertions.Throws<TException>(action);

    // public static void Returns<T>(T expected, T actual)
    //     => ServiceAssertions.Returns(expected, actual);

    // // Repository Assertions
    // public static void EntityEqual<T>(T expected, T actual)
    //     => RepositoryAssertions.EntityEqual(expected, actual);

    // public static void Exists(bool exists)
    //     => RepositoryAssertions.Exists(exists);

    // public static void NotExists(bool exists)
    //     => RepositoryAssertions.NotExists(exists);

    // // Common Assertions
    // public static void Equal<T>(T expected, T actual)
    //     => CommonAssertions.Equal(expected, actual);

    // public static void True(bool condition)
    //     => CommonAssertions.True(condition);

    // public static void False(bool condition)
    //     => CommonAssertions.False(condition);
}
