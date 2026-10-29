using Multitool.Domain.Enums;
using Multitool.Domain.Interfaces;

namespace Multitool.Domain.Entities.Category;

public class Category : IUserEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public List<AppModule> ApplicableModules { get; set; } = [];
    public bool IsDeleted { get; set; }
}
