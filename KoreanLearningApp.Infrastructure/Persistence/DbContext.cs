using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
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
        await _connection.CreateTableAsync<KrDictEntity>();
        await _connection.CreateTableAsync<KrDictSenseEntity>();
        await _connection.CreateTableAsync<AudioEntity>();
        await _connection.CreateTableAsync<LangInfoEntity>();
        await _connection.CreateTableAsync<WordProgressEntity>();
        await _connection.CreateTableAsync<ReviewLogEntity>();
        await _connection.CreateTableAsync<SavedWordEntity>();

        return _connection;
    }
}