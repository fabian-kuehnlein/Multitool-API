using Multitool.Domain.Entities.Category;

namespace Multitool.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetCategoriesAsync();
}
