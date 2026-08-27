using System.Text.Json;
using Microsoft.Extensions.Logging;
using Multitool.Domain.Entities.Calendar;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.ApiClients.Models;

namespace Multitool.Infrastructure.ApiClients;

public class CalendarApiClient(HttpClient httpClient, ILogger<CalendarApiClient> logger) : ICalendarApiClient
{
    public async Task<List<Holiday>> GetHolidaysAsync(string year)
    {
        logger.LogInformation("Fetching holidays for year {Year}", year);

        try
        {
            var url = $"?years={year}&states=by";
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<HolidayResponse>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var holidays = data?.Feiertage?.Select(item => new Holiday
            {
                Name = item.Fname,
                Date = DateTime.Parse(item.Date)
            }).ToList() ?? new List<Holiday>();

            logger.LogInformation("Fetched {HolidayCount} holiday(s) for year {Year}", holidays.Count, year);
            return holidays;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch holidays for year {Year}", year);
            throw;
        }
    }
}