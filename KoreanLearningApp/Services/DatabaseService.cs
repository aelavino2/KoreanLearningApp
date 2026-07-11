using KoreanLearningApp.Models;
using SQLite;
using System.Text.Encodings.Web;
using System.Text.Json;

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

    public async Task<ImportResult> ImportWordsAsync(List<WordImportDto> incoming)
    {
        await Init();
        var existing = await _connection!.Table<Word>().ToListAsync();

        var toInsert = new List<Word>();
        var skipped = new List<string>();
        var seenInBatch = new HashSet<string>();

        foreach (var dto in incoming)
        {
            if (string.IsNullOrWhiteSpace(dto.Korean) || string.IsNullOrWhiteSpace(dto.TranslationRu))
            {
                skipped.Add($"{dto.Korean} — пропущено (нет корейского слова или перевода)");
                continue;
            }

            var key = MakeKey(dto.Korean, dto.TranslationRu);

            if (!seenInBatch.Add(key))
            {
                skipped.Add($"{dto.Korean} ({dto.TranslationRu}) — повтор внутри присланного списка");
                continue;
            }

            bool isDuplicate = existing.Any(w => MakeKey(w.Korean, w.TranslationRu) == key);
            if (isDuplicate)
            {
                skipped.Add($"{dto.Korean} ({dto.TranslationRu}) — уже есть в базе");
                continue;
            }

            toInsert.Add(new Word
            {
                Korean = dto.Korean.Trim(),
                TranscriptionRu = dto.TranscriptionRu.Trim(),
                TranscriptionEn = dto.TranscriptionEn.Trim(),
                TranslationRu = dto.TranslationRu.Trim(),
                TranslationEn = dto.TranslationEn.Trim(),
                RuleExplanation = dto.Rule?.Trim() ?? string.Empty
            });
        }

        if (toInsert.Count > 0)
            await _connection!.InsertAllAsync(toInsert);

        return new ImportResult(toInsert.Count, skipped);
    }

    public async Task<string> ExportWordsAsJsonAsync()
    {
        await Init();
        var words = await _connection!.Table<Word>().ToListAsync();

        var dtos = words.Select(w => new WordImportDto
        {
            Korean = w.Korean,
            TranscriptionRu = w.TranscriptionRu,
            TranscriptionEn = w.TranscriptionEn,
            TranslationRu = w.TranslationRu,
            TranslationEn = w.TranslationEn,
            Rule = w.RuleExplanation
        }).ToList();

        return JsonSerializer.Serialize(dtos, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // чтобы корейские/русские символы не превращались в \uXXXX
        });
    }

    private static string MakeKey(string korean, string translationRu)
        => $"{korean.Trim().ToLowerInvariant()}|{translationRu.Trim().ToLowerInvariant()}";

    public async Task<int> DeleteWordAsync(Word word)
    {
        await Init();
        return await _connection!.DeleteAsync(word);
    }
}