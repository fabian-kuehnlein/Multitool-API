using Multitool.Application.Models.Category;
using Multitool.Domain.Entities.Category;
using Multitool.Domain.Enums;

namespace Multitool.Tests.Shared;

public static class CategoryTestData
{
    public static List<AppModule> DefaultApplicableModules => [AppModule.Todo, AppModule.Calendar];

    public static Category DefaultCategory => new()
    {
        Id = 1,
        Name = "Arbeit",
        Color = "#FF0000",
        ApplicableModules = DefaultApplicableModules
    };

    public static CategoryDto DefaultCategoryDto => new(DefaultCategory.Id, DefaultCategory.Name, DefaultCategory.Color, DefaultApplicableModules);

    public static CreateCategoryDto DefaultCreateCategoryDto => new(DefaultCategory.Name, DefaultCategory.Color, DefaultApplicableModules);

    public static UpdateCategoryDto DefaultUpdateCategoryDto => new("Updated Category", "#00FF00", DefaultApplicableModules);
}
