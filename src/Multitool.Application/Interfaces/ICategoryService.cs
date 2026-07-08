using Multitool.Domain.Entities.Category;

namespace Multitool.Application.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync();
}
