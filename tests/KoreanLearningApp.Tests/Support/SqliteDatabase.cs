using KoreanLearningApp.Infrastructure.Persistence;

namespace KoreanLearningApp.Tests.Support;

/// <summary>
/// Отдельный файл SQLite во временной папке на каждый тест.
/// </summary>
public sealed class SqliteDatabase : IAsyncDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"korean_test_{Guid.NewGuid():N}.db");

    public SqliteDatabase()
    {
        Context = new DbContext(_path);
    }

    public DbContext Context { get; }

    public async Task<int> CountAsync(string table)
    {
        var db = await Context.GetConnectionAsync();
        return await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {table}");
    }

    public async ValueTask DisposeAsync()
    {
        var db = await Context.GetConnectionAsync();
        await db.CloseAsync();
        File.Delete(_path);
    }
}
