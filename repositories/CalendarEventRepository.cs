using MySqlConnector;
using Microsoft.Extensions.Configuration;
using CalendarApp.Models;

public class CalendarEventRepository
{
    private readonly string _connectionString;

    public CalendarEventRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MariaDBConnection");
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

    public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end)
    {
        var events = new List<CalendarEvent>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand(@"
                SELECT eventId, eventTitle, eventNote, startDateTime, endDateTime, allDay, categoryId
                FROM calendar_event
                WHERE startDateTime < @endDate AND endDateTime > @startDate", connection);
                
            command.Parameters.AddWithValue("@startDate", start);
            command.Parameters.AddWithValue("@endDate", end);

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
                        IsAllDay = reader.GetBoolean("allDay"),
                        CategoryId = reader.GetInt32("categoryId")
                    });
                }
            }
        }

        return events;
    }

    public async Task InsertEventAsync(CalendarEvent calendarEvent)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand("INSERT INTO calendar_event (eventTitle, eventNote, startDateTime, endDateTime, categoryId) VALUES (@eventTitle, @eventNote, @startDateTime, @endDateTime, @categoryId)", connection);
            command.Parameters.AddWithValue("@eventTitle", calendarEvent.EventTitle);
            command.Parameters.AddWithValue("@eventNote", (object)calendarEvent.EventNote ?? DBNull.Value);
            command.Parameters.AddWithValue("@startDateTime", calendarEvent.StartDateTime);
            command.Parameters.AddWithValue("@endDateTime", calendarEvent.EndDateTime);
            command.Parameters.AddWithValue("@categoryId", calendarEvent.CategoryId);

            await command.ExecuteNonQueryAsync();
        }
    }
}
