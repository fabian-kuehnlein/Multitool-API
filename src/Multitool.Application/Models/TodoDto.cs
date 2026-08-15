namespace Multitool.Application.Models;

public class TodoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public bool IsDone { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime? CompletedDateTime { get; set; }
}
