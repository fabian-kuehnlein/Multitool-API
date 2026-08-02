using System.Text.Json;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.ApiClients.Models;

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

        return data?.Feiertage?.Select(item => new Holiday
        {
            Name = item.Fname,
            Date = DateTime.Parse(item.Date)
        }).ToList() ?? new List<Holiday>();
    }
}