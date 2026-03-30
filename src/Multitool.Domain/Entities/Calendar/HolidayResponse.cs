namespace Multitool.Domain.Entities.Calendar;

public class HolidayResponse
{
    public required string Status { get; set; }
    public required List<HolidayRaw> Feiertage { get; set; }
}
