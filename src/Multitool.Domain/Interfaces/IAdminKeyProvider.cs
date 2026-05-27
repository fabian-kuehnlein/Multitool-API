using Multitool.Domain.Entities.Config;

namespace Multitool.Domain.Interfaces;

public interface IAdminKeyProvider
{
    string GetAdminKey();
}