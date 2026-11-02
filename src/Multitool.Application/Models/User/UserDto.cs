using Multitool.Domain.Enums;

namespace Multitool.Application.Models.User;

/// <summary>
/// Serializable view of a user for the administration area.
/// </summary>
public record UserDto(
    int Id,
    string Username,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt);
