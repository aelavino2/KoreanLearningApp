using KoreanLearningApp.Infrastructure.Persistence.Entities;
using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence;

public class DbContext
{
    private readonly string _databasePath;
    private SQLiteAsyncConnection? _connection;

    public DbContext(string databasePath)
    {
        _databasePath = databasePath;
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        _connection = new SQLiteAsyncConnection(_databasePath, DatabaseConstants.Flags);
        await _connection.CreateTableAsync<WordEntity>();

        return _connection;
    }
}