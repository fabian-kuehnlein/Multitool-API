using Multitool.Domain.Entities.Categories;

namespace Multitool.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<List<Category>> GetCategoriesAsync();
    Task<int> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
}
