namespace Multitool.Application.Models.Account;

/// <summary>
/// Request body for setting the authenticated user's own username and password
/// after logging in with a temporary password.
/// </summary>
public record SetupAccountCredentialsDto(string Username, string Password);
