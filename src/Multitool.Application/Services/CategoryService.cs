using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Domain.Entities.Category;
using Multitool.Application.Models.Category;

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

    public async Task<int> CreateCategoryAsync(CreateCategoryDto newCategory)
        => await categoryRepository.CreateCategoryAsync(newCategory.Adapt<Category>());

    public async Task UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
    {
        var existing = await categoryRepository.GetByIdAsync(id);

        if (existing is null)
            throw new NotFoundException($"Category with id {id} not found");

        existing.Name = updateCategoryDto.Name;
        existing.Color = updateCategoryDto.Color;
        existing.ApplicableModules = updateCategoryDto.ApplicableModules;

        await categoryRepository.UpdateCategoryAsync(existing);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var categories = await categoryRepository.GetCategoriesAsync();

        if (categories.Count == 1)
            throw new CannotDeleteLastCategoryException("Cannot delete the last remaining category.");

        var category = await categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new NotFoundException($"Category with id {id} not found");

        await categoryRepository.DeleteCategoryAsync(category);
    }
}
