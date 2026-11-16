using Multitool.Application.Models.Account;

namespace Multitool.Application.Interfaces;

public interface IAccountService
{
    Task VerifyAccountCredentialsAsync(VerifyAccountCredentialsRequest request);
    Task<string> SetupAccountCredentialsAsync(SetupAccountCredentialsRequest request);
    Task<string> ChangeAccountCredentialsAsync(ChangeAccountCredentialsRequest request);
}
