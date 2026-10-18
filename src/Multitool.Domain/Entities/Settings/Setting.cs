namespace Multitool.Domain.Entities.Settings;

/// <summary>
/// A single global setting stored as a key/value pair for a specific user.
/// The value is always persisted as a string and interpreted by the backend
/// according to the registered setting definition.
/// </summary>
public class Setting
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public DateTime UpdatedAt { get; set; }
}
