using Multitool.Application.Interfaces;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = await categoryRepository.GetCategoriesAsync();

        if (categories is null || categories.Count <= 0)
            throw new NotFoundException("No categories found");

        return categories;
    }
}
