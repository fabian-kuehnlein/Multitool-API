using Multitool.Application.Models.Category;

namespace Multitool.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryModel>> GetCategoriesAsync();
    Task<int> CreateCategoryAsync(CreateCategoryRequest request);
    Task UpdateCategoryAsync(int id, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(int id);
}
