using Multitool.Domain.Enums;

namespace Multitool.Application.Models.User;

/// <summary>
/// Request body for creating a new user. The initial password is a temporary password
/// that the user should change after the first login.
/// </summary>
public record CreateUserDto(
    string Username,
    string Password,
    UserRole Role);
