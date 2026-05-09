namespace Multitool.Infrastructure.Authentification;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}