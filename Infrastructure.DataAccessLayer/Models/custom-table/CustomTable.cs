namespace MultitoolApi.Infrastructure.DataAccessLayer.Models.CustomTable;

public class CustomTable
{
    public long TableId { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public ICollection<CustomColumn> Columns { get; set; } = [];
    public ICollection<CustomRow> Rows { get; set; } = [];
}