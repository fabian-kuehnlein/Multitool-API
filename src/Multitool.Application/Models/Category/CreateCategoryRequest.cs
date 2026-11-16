using Multitool.Domain.Enums;

namespace Multitool.Application.Models.Category;

public record CreateCategoryRequest
(
    string Name,
    string Color,
    List<AppModule> ApplicableModules
);
