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
        await _connection.CreateTableAsync<WordSenseEntity>();
        await _connection.CreateTableAsync<KrDictEntity>();
        await _connection.CreateTableAsync<KrDictSenseEntity>();
        await _connection.CreateTableAsync<AudioEntity>();
        await _connection.CreateTableAsync<LangInfoEntity>();

        return _connection;
    }

    public async Task SeedIfEmptyAsync()
    {
        var connection = await GetConnectionAsync();
        var count = await connection.Table<WordEntity>().CountAsync();
        if (count > 0)
            return;

        foreach (var word in WordSeeder.GetSeedWords())
        {
            var wordEntity = word.ToEntity();
            await connection.InsertAsync(wordEntity);

            var senseEntities = word.Senses.Select(s => s.ToEntity(wordEntity.Id)).ToList();
            if (senseEntities.Count > 0)
                await connection.InsertAllAsync(senseEntities);
        }
    }
}