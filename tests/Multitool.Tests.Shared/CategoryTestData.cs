using Multitool.Application.Models.Category;
using Multitool.Domain.Entities.Category;

namespace Multitool.Tests.Shared;

public static class CategoryTestData
{
    public static Category DefaultCategory => new() { Id = 1, Name = "Arbeit", Color = "#FF0000" };

    public static CategoryDto DefaultCategoryDto => new(DefaultCategory.Id, DefaultCategory.Name, DefaultCategory.Color);

    public static CreateCategoryDto DefaultCreateCategoryDto => new(DefaultCategory.Name, DefaultCategory.Color);

    public static UpdateCategoryDto DefaultUpdateCategoryDto => new("Updated Category", "#00FF00");
}
