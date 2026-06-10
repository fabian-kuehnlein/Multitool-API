namespace Multitool.Domain.Entities.Todo;

public class Todo
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required int CategoryId { get; set; }
    public required bool IsDone { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreationDateTime { get; set; }
}
