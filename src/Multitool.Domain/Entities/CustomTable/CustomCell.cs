namespace Multitool.Domain.Entities.CustomTable;

public class CustomCell {
    public long RowId { get; set; }
    public CustomRow Row { get; set; } = default!;
    public long ColumnId { get; set; }
    public CustomColumn Column { get; set; } = default!;
    public string?  ValString { get; set; }
    public long?    ValInt    { get; set; }
    public decimal? ValDec    { get; set; }
    public DateTime? ValDate  { get; set; }
    public bool?    ValBool   { get; set; }
}