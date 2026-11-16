using Mapster;
using Multitool.Application.Interfaces;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;
using Multitool.Domain.Entities.Category;
using Multitool.Application.Models.Category;

namespace Multitool.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        var categories = await categoryRepository.GetCategoriesAsync();

        if (categories is null || categories.Count <= 0)
            throw new NotFoundException("No categories found");

        return categories.Adapt<List<CategoryModel>>();
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryRequest request)
        => await categoryRepository.CreateCategoryAsync(request.Adapt<Category>());

    public async Task UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var existing = await categoryRepository.GetByIdAsync(id);

        if (existing is null)
            throw new NotFoundException($"Category with id {id} not found");

        existing.Name = request.Name;
        existing.Color = request.Color;
        existing.ApplicableModules = request.ApplicableModules;

        await categoryRepository.UpdateCategoryAsync(existing);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var categories = await categoryRepository.GetCategoriesAsync();

        if (categories.Count(c => !c.IsDeleted) <= 1)
            throw new CannotDeleteLastCategoryException("Cannot delete the last remaining category.");

        var category = await categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new NotFoundException($"Category with id {id} not found");

        category.IsDeleted = true;

        await categoryRepository.UpdateCategoryAsync(category);
    }
}
