using Microsoft.Extensions.Configuration;
using Multitool.Domain.Interfaces;

public class AdminKeyProvider(IConfiguration config) : IAdminKeyProvider
{
    public string GetAdminKey()
    {
        return config["AdminKey"] ?? "";
    }
}
