using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;
using Multitool.Tests.Shared;

namespace Multitool.Infrastructure.Tests;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly AppDbContext Context;

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var userContextMock = new Mock<IUserContext>();
        userContextMock.SetupGet(c => c.UserId).Returns(AuthTestData.DefaultUser.Id);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options, userContextMock.Object);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
