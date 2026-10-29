namespace Multitool.Domain.Enums;

/// <summary>
/// Role of a user. <see cref="Admin"/> only grants access to user administration,
/// never to the business data of other users.
/// </summary>
public enum UserRole
{
    User,
    Admin
}
