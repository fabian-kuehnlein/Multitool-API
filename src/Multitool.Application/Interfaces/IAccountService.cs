using Multitool.Application.Models.Account;

namespace Multitool.Application.Interfaces;

public interface IAccountService
{
    Task VerifyAccountCredentialsAsync(VerifyAccountCredentialsDto dto);
    Task<string> SetupAccountCredentialsAsync(SetupAccountCredentialsDto dto);
    Task<string> ChangeAccountCredentialsAsync(ChangeAccountCredentialsDto dto);
}
