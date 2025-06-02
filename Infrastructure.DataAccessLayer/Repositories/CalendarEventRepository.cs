using MySqlConnector;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.Webapi.Models;
using MultitoolApi.Businesslogic.Models;
using System.Text.Json;

public class CalendarEventRepository : ICalendarEventRepository
{
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;

    public CalendarEventRepository(IConfiguration configuration, HttpClient httpClient)
    {
        _connectionString = configuration.GetConnectionString("MariaDBConnection") 
            ?? throw new InvalidOperationException("Connection string 'MariaDBConnection' not found.");
        _httpClient = httpClient;
    }

    public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end)
    {
        var events = new List<CalendarEvent>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand(@"
                SELECT eventId, eventTitle, eventNote, startDateTime, endDateTime, allDay, categoryId, recurrenceRule, recurrenceEnd
                FROM calendar_event
                WHERE 
                    (startDateTime < @endDate AND (endDateTime IS NULL OR endDateTime > @startDate))
                    OR
                    (recurrenceRule IS NOT NULL AND (recurrenceEnd IS NULL OR recurrenceEnd >= @startDate))                
                ", connection);

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
                        EventNote = reader.IsDBNull(reader.GetOrdinal("eventNote"))
                            ? null
                            : reader.GetString("eventNote"),
                        StartDateTime = reader.GetDateTime("startDateTime"),
                        EndDateTime = reader.IsDBNull(reader.GetOrdinal("endDateTime"))
                            ? (DateTime?)null
                            : reader.GetDateTime("endDateTime"),
                        IsAllDay = reader.GetBoolean("allDay"),
                        CategoryId = reader.GetInt32("categoryId"),
                        RecurrenceRule = reader.IsDBNull(reader.GetOrdinal("recurrenceRule"))
                            ? null
                            : reader.GetString("recurrenceRule"),
                        RecurrenceEnd = reader.IsDBNull(reader.GetOrdinal("recurrenceEnd"))
                            ? (DateTime?)null
                            : reader.GetDateTime("recurrenceEnd")
                    });
                }
            }
        }

        return events;
    }

    public async Task InsertEventAsync(CreateCalendarEventDAO calendarEvent)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand(
                "INSERT INTO calendar_event " +
                "(eventTitle, eventNote, startDateTime, endDateTime, allDay, categoryId) " +
                "VALUES " +
                "(@eventTitle, @eventNote, @startDateTime, @endDateTime, @IsAllDay, @categoryId)",
                connection
            );
            command.Parameters.AddWithValue("@eventTitle", calendarEvent.EventTitle);
            command.Parameters.AddWithValue("@eventNote", calendarEvent.EventNote == null ? DBNull.Value : (object)calendarEvent.EventNote);
            command.Parameters.AddWithValue("@startDateTime", calendarEvent.StartDateTime);
            command.Parameters.AddWithValue("@endDateTime", calendarEvent.EndDateTime);
            command.Parameters.AddWithValue("@IsAllDay", calendarEvent.IsAllDay);
            command.Parameters.AddWithValue("@categoryId", calendarEvent.CategoryId);

            await command.ExecuteNonQueryAsync();
        }
    }

public async Task<CalendarEventDAO> UpdateEventAsync(CalendarEventDAO updateEvent)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand(
                "UPDATE calendar_event SET " +
                "eventTitle = @eventTitle, eventNote = @eventNote, startDateTime = @startDateTime, " +
                "endDateTime = @endDateTime, allDay = @IsAllDay, categoryId = @categoryId " +
                "WHERE eventId = @eventId",
                connection
            );
            command.Parameters.AddWithValue("@eventId", updateEvent.EventId);
            command.Parameters.AddWithValue("@eventTitle", updateEvent.EventTitle);
            command.Parameters.AddWithValue("@eventNote", updateEvent.EventNote == null ? DBNull.Value : (object)updateEvent.EventNote);
            command.Parameters.AddWithValue("@startDateTime", updateEvent.StartDateTime);
            command.Parameters.AddWithValue("@endDateTime", updateEvent.EndDateTime);
            command.Parameters.AddWithValue("@IsAllDay", updateEvent.IsAllDay);
            command.Parameters.AddWithValue("@categoryId", updateEvent.CategoryId);
            
            await command.ExecuteNonQueryAsync();

            return updateEvent;
        }
    }

    public async Task DeleteEventAsync(int eventId)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand("DELETE FROM calendar_event WHERE eventId = @eventId", connection);
            command.Parameters.AddWithValue("@eventId", eventId);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = new List<Category>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new MySqlCommand("SELECT categoryId, category_name FROM category ORDER BY categoryId ASC", connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    categories.Add(new Category
                    {
                        CategoryId = reader.GetInt32("categoryId"),
                        CategoryName = reader.GetString("category_name")
                    });
                }
            }
        }

        return categories;
    }

    public async Task<List<HolidayDAO>> GetHolidaysAsync(string year)
    {
        var url = $"?years={year}&states=by";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<HolidayResponse>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return data?.Feiertage?.Select(item => new HolidayDAO
        {
            HolidayName = item.Fname,
            HolidayDate = DateTime.Parse(item.Date)
        }).ToList() ?? new List<HolidayDAO>();
    }
}
