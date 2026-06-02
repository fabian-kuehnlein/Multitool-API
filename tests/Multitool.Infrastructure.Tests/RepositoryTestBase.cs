using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Multitool.Infrastructure.Data;

namespace Multitool.Infrastructure.Tests;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly AppDbContext Context;

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
