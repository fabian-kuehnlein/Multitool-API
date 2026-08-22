using Multitool.Domain.Entities.Category;

namespace Multitool.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetCategoriesAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<int> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
}
