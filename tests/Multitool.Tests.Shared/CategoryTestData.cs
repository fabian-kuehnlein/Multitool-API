using Multitool.Application.Models.Category;
using Multitool.Domain.Entities.Categories;
using Multitool.Domain.Enums;

namespace Multitool.Tests.Shared;

public static class CategoryTestData
{
    public static List<AppModule> DefaultApplicableModules => [AppModule.Todo, AppModule.Calendar];

    public static List<AppModule> KanbanApplicableModules => [AppModule.Kanban];

    public static Category KanbanCategory => new()
    {
        Id = 2,
        UserId = AuthTestData.DefaultUser.Id,
        Name = "Kanban",
        Color = "#00FF00",
        ApplicableModules = KanbanApplicableModules,
        IsDeleted = false
    };

    public static Category CreateKanbanCategory(string name) => new()
    {
        UserId = AuthTestData.DefaultUser.Id,
        Name = name,
        Color = "#FF0000",
        ApplicableModules = KanbanApplicableModules,
        IsDeleted = false
    };

    public static Category DefaultCategory => new()
    {
        Id = 1,
        UserId = AuthTestData.DefaultUser.Id,
        Name = "Arbeit",
        Color = "#FF0000",
        ApplicableModules = DefaultApplicableModules,
        IsDeleted = false
    };

    public static Category DeletedCategory => new()
    {
        Id = 3,
        UserId = AuthTestData.DefaultUser.Id,
        Name = "Gelöscht",
        Color = "#0000FF",
        ApplicableModules = DefaultApplicableModules,
        IsDeleted = true
    };

    public static CategoryModel DefaultCategoryModel => new(DefaultCategory.Id, DefaultCategory.Name, DefaultCategory.Color, DefaultApplicableModules, false);

    public static CreateCategoryRequest DefaultCreateCategoryRequest => new(DefaultCategory.Name, DefaultCategory.Color, DefaultApplicableModules);

    public static UpdateCategoryRequest DefaultUpdateCategoryRequest => new("Updated Category", "#00FF00", DefaultApplicableModules);
}
