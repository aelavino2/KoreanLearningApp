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
        return _connection;
    }

    public async Task SeedIfEmptyAsync()
    {
        var connection = await GetConnectionAsync();
        var count = await connection.Table<WordEntity>().CountAsync();
        if (count > 0)
            return;

        var entities = WordSeeder.GetSeedWords()
            .Select(w => w.ToEntity())
            .ToList();

        await connection.InsertAllAsync(entities);
    }
}