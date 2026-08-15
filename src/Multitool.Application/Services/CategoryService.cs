using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Application.Models;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await categoryRepository.GetCategoriesAsync();

        if (categories is null || categories.Count <= 0)
            throw new NotFoundException("No categories found");

        return categories.Adapt<List<CategoryDto>>();
    }
}
