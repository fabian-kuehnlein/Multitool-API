namespace Multitool.Application.Interfaces;

public interface IAuthenticationService
{
    Task RegisterAsync(string username, string password);
    Task<string> LoginAsync(string username, string password);
}
