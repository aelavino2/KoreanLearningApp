using SQLite;
using KoreanLearningApp.Models;

namespace KoreanLearningApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _connection;

    private async Task Init()
    {
        if (_connection is not null)
            return;

        var dbPath = GetDbPath();
        _connection = new SQLiteAsyncConnection(dbPath);
        await _connection.CreateTableAsync<Word>();

        // Если база пустая — заполняем стартовым набором слов
        var count = await _connection.Table<Word>().CountAsync();
        if (count == 0)
        {
            await _connection.InsertAllAsync(WordSeeder.GetSeedWords());
        }
    }

    private static string GetDbPath()
    {
#if WINDOWS
        // Во время разработки на Windows храним базу прямо в папке проекта,
        // чтобы можно было легко открыть её в DB Browser for SQLite
        var folder = @"D:\c#\KoreanLearningApp\Database";
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "korean_learning.db3");
#else
        // На мобильных платформах используем приватное хранилище приложения
        return Path.Combine(FileSystem.AppDataDirectory, "korean_learning.db3");
#endif
    }

    public async Task<List<Word>> GetWordsAsync()
    {
        await Init();
        return await _connection!.Table<Word>().ToListAsync();
    }

    public async Task<int> SaveWordAsync(Word word)
    {
        await Init();
        if (word.Id != 0)
            return await _connection!.UpdateAsync(word);

        return await _connection!.InsertAsync(word);
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        await Init();
        return await _connection!.DeleteAsync(word);
    }
}