namespace Multitool.Application.Models.Account;

/// <summary>
/// Request body for a regular change of the authenticated user's own account
/// data. The current password confirms the change; a new username and/or a
/// new password can be supplied.
/// </summary>
public record ChangeAccountCredentialsRequest(
    string CurrentPassword,
    string? NewUsername,
    string? NewPassword);
