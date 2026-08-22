using FluentAssertions;

internal static class GeneralAssertions
{
    /// <summary>
    /// Asserts that a boolean condition is true.
    /// </summary>
    public static void IsTrue(bool condition)
    {
        condition.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that a boolean condition is false.
    /// </summary>
    public static void IsFalse(bool condition)
    {
        condition.Should().BeFalse();
    }
}