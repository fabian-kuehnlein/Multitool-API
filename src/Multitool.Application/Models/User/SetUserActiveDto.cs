namespace Multitool.Application.Models.User;

/// <summary>
/// Request body for activating or deactivating a user.
/// </summary>
public record SetUserActiveDto(bool IsActive);
