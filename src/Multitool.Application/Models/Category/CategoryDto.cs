using Multitool.Domain.Enums;

namespace Multitool.Application.Models.Category;

public record CategoryDto
(
    int Id,
    string Name,
    string Color,
    List<AppModule> ApplicableModules,
    bool IsDeleted
);
