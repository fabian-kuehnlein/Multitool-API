namespace Multitool.Domain.Interfaces;

/// <summary>
/// Provides the id of the currently authenticated user. The id is always taken from the
/// JWT and never from a request parameter.
/// </summary>
public interface IUserContext
{
    int UserId { get; }
}
