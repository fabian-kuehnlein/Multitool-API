using MySqlConnector;
using MultitoolApi.DataAccessLayer.Models;
using MultitoolApi.Businesslogic.Models;
using System.Text.Json;
using MultitoolApi.WebApi.Models.CustomTable;
using System.Data;

public class CustomTableRepository : ICustomTableRepository
{
    private readonly string _connectionString;
    private readonly ILogger<CalendarEventRepository> _logger;

    public CustomTableRepository(IConfiguration configuration, ILogger<CalendarEventRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'MariaDBConnection' not found.");
        _logger = logger;
    }

    public async Task<IReadOnlyList<TableOverview>> GetTableListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT table_id AS TableId, name AS TableName
            FROM custom_table
            ORDER BY name;
        """;

        var list = new List<TableOverview>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(sql, connection);

        await using var r = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);

        while (await r.ReadAsync(cancellationToken))
        {
            var table = new TableOverview(
                TableId: r.GetInt64("TableId"),
                Name: r.GetString("TableName")
            );

            list.Add(table);
        }

        return list;
    }
}