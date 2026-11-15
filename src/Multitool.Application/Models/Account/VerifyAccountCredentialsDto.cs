namespace Multitool.Application.Models.Account;

/// <summary>
/// Request body for confirming the authenticated user's current credentials
/// before they are allowed to enter new account data.
/// </summary>
public record VerifyAccountCredentialsDto(string Username, string Password);
