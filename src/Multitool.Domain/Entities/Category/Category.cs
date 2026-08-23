using Multitool.Domain.Enums;

namespace Multitool.Domain.Entities.Category;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public List<AppModule> ApplicableModules { get; set; } = [];
    public bool IsDeleted { get; set; }
}

