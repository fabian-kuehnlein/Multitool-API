using FluentAssertions;

namespace Multitool.Tests.Shared.Assertions;

internal static class ServiceAssertions
{
    /// <summary>
    /// Asserts that the given async action throws the expected exception type.
    /// </summary>
    public static async Task ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        await action.Should().ThrowAsync<TException>();
    }

    /// <summary>
    /// Asserts that the given async action throws the expected exception type
    /// and that the message contains the expected substring.
    /// </summary>
    public static async Task ThrowsAsync<TException>(Func<Task> action, string expectedMessagePart)
        where TException : Exception
    {
        await action.Should()
            .ThrowAsync<TException>()
            .WithMessage($"*{expectedMessagePart}*");
    }

    /// <summary>
    /// Asserts that two objects are equivalent (deep comparison).
    /// </summary>
    public static void AreEqual<T>(T actual, T expected)
    {
        actual.Should().BeEquivalentTo(expected, options =>
            options.ExcludingMissingMembers());
    }

    /// <summary>
    /// Asserts that a DateTime is close to another DateTime within a tolerance.
    /// Useful for CreationDateTime, UpdatedAt, etc.
    /// </summary>
    public static void CloseTo(DateTime actual, DateTime expected, TimeSpan tolerance)
    {
        actual.Should().BeCloseTo(expected, tolerance);
    }
}
