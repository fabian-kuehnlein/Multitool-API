using Microsoft.AspNetCore.Mvc;

namespace Multitool.Tests.Shared.Assertions;

public static class AssertEx
{
    // General Assertions
    /// <inheritdoc cref="GeneralAssertions.IsTrue(bool)" />
    public static void IsTrue<T>(bool condition)
        => GeneralAssertions.IsTrue(condition);

    /// <inheritdoc cref="GeneralAssertions.IsFalse(bool)" />
    public static void IsFalse<T>(bool condition)
        => GeneralAssertions.IsFalse(condition);

    // Controller Assertions
    /// <inheritdoc cref="ControllerAssertions.Ok{T}(IActionResult, T)" />
    public static void Ok<T>(IActionResult result, T expected)
        => ControllerAssertions.Ok(result, expected);

    /// <inheritdoc cref="ControllerAssertions.Created{T}(IActionResult, T)" />
    public static void Created<T>(IActionResult result, T expected)
        => ControllerAssertions.Created(result, expected);

    /// <inheritdoc cref="ControllerAssertions.NoContent(IActionResult)" />
    public static void NoContent(IActionResult result)
        => ControllerAssertions.NoContent(result);

    /// <inheritdoc cref="ControllerAssertions.BadRequest(IActionResult)" />
    public static void BadRequest(IActionResult result)
        => ControllerAssertions.BadRequest(result);

    /// <inheritdoc cref="ControllerAssertions.NotFound(IActionResult)" />
    public static void NotFound(IActionResult result)
        => ControllerAssertions.NotFound(result);

    // Service Assertions
    /// <inheritdoc cref="ServiceAssertions.ThrowsAsync{TException}(Func{Task})" />
    public static Task Throws<TException>(Func<Task> action)
        where TException : Exception
        => ServiceAssertions.ThrowsAsync<TException>(action);

    /// <inheritdoc cref="ServiceAssertions.AreEqual{T}(T, T)" />
    public static void AreEqual<T>(T expected, T actual)
        => ServiceAssertions.AreEqual(expected, actual);

    /// <inheritdoc cref="ServiceAssertions.CloseTo(DateTime, DateTime, TimeSpan)" />
    public static void CloseTo(DateTime actual, DateTime expected, TimeSpan tolerance)
        => ServiceAssertions.CloseTo(actual, expected, tolerance);

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
