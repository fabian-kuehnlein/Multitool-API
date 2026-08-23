using Multitool.Domain.Enums;

namespace Multitool.Application.Models.Category;

public record CreateCategoryDto
(
    string Name,
    string Color,
    List<AppModule> ApplicableModules
);
