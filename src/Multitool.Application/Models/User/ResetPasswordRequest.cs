namespace Multitool.Application.Models.User;

/// <summary>
/// Request body for setting a new temporary password.
/// </summary>
public record ResetPasswordRequest(string Password);
