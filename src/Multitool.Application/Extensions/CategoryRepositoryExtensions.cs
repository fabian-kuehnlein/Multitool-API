using Multitool.Domain.Enums;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Application.Extensions;

internal static class CategoryRepositoryExtensions
{
    public static async Task<Domain.Entities.Category.Category> GetApplicableCategoryAsync(
        this ICategoryRepository categoryRepository, int categoryId, AppModule module)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);

        if (category is null)
            throw new NotFoundException($"Category with ID {categoryId} not found.");

        if (category.IsDeleted || !category.ApplicableModules.Contains(module))
            throw new CategoryNotAvailableForModuleException(
                $"Category '{category.Name}' is not available for the {module} module.");

        return category;
    }
}
