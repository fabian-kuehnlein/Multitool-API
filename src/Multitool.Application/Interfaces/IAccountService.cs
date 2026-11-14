using Multitool.Application.Models.Account;

namespace Multitool.Application.Interfaces;

public interface IAccountService
{
    Task<string> UpdateAccountCredentialsAsync(UpdateAccountCredentialsDto dto);
}
