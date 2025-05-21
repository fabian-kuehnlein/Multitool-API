using MySqlConnector;
using Microsoft.Extensions.Configuration;
using CalendarApp.Models;

public class CalendarEventRepository
{
    private readonly string _connectionString;

    public CalendarEventRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MariaDb");
    }

    public async Task<List<CalendarEvent>> GetAllEventsAsync()
    {
        var events = new List<CalendarEvent>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand("SELECT eventId, eventTitle, eventNote, startDateTime, endDateTime, categoryId FROM calendar_event", connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    events.Add(new CalendarEvent
                    {
                        EventId = reader.GetInt32("eventId"),
                        EventTitle = reader.GetString("eventTitle"),
                        EventNote = reader.IsDBNull(reader.GetOrdinal("eventNote")) ? null : reader.GetString("eventNote"),
                        StartDateTime = reader.GetDateTime("startDateTime"),
                        EndDateTime = reader.GetDateTime("endDateTime"),
                        CategoryId = reader.GetInt32("categoryId")
                    });
                }
            }
        }

        return events;
    }
}
