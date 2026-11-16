using Multitool.Domain.Enums;

namespace Multitool.Application.Models.Category;

public record UpdateCategoryRequest
(
    string Name,
    string Color,
    List<AppModule> ApplicableModules
);
