using Multitool.Application.Models.Category;

namespace Multitool.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<int> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
    Task DeleteCategoryAsync(int id);
}
