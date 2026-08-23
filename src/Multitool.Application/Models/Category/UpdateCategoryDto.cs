using Multitool.Domain.Enums;

namespace Multitool.Application.Models.Category;

public record UpdateCategoryDto
(
    string Name,
    string Color,
    List<AppModule> ApplicableModules
);
