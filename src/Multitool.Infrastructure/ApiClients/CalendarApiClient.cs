using System.Text.Json;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Exceptions;
using Multitool.Domain.Interfaces;

namespace Multitool.Infrastructure.ApiClients;

public class CalendarApiClient(HttpClient httpClient) : ICalendarApiClient
{
    public async Task<List<Holiday>> GetHolidaysAsync(string year)
    {
        var url = $"?years={year}&states=by";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<HolidayResponse>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data is null || data.Feiertage is null || data.Feiertage.Count <= 0)
            throw new NotFoundException($"No holidays found for year {year}");

        return data?.Feiertage?.Select(item => new Holiday
        {
            Name = item.Fname,
            Date = DateTime.Parse(item.Date)
        }).ToList() ?? new List<Holiday>();
    }
}