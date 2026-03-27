using Multitool.Domain.Entities.Calendar;

namespace Multitool.Domain.Interfaces;

public interface ICalendarApiClient
{
    Task<List<Holiday>> GetHolidaysAsync(string year);
}