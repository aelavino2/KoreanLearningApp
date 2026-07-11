using KoreanLearningApp.Models;
using SQLite;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace KoreanLearningApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _connection;
    private const string BackupFileName = "words_backup.json";

    private async Task Init()
    {
        if (_connection is not null)
            return;

        var dbPath = GetDbPath();
        _connection = new SQLiteAsyncConnection(dbPath);
        await _connection.CreateTableAsync<Word>();

        var count = await _connection.Table<Word>().CountAsync();
        if (count == 0)
        {
            await _connection.InsertAllAsync(WordSeeder.GetSeedWords());
            await AutoSaveBackupAsync();
        }
    }

    private static string GetDbPath()
    {
#if WINDOWS
        var folder = @"D:\c#\KoreanLearningApp\Database";
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "korean_learning.db3");
#else
        return Path.Combine(FileSystem.AppDataDirectory, "korean_learning.db3");
#endif
    }

    // Папка, куда автоматически пишется резервная JSON-копия при каждом изменении базы
    public static string GetBackupFolder()
    {
#if WINDOWS
        var folder = @"D:\c#\KoreanLearningApp\Database";
#else
        var folder = FileSystem.AppDataDirectory;
#endif
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetBackupFilePath()
        => Path.Combine(GetBackupFolder(), BackupFileName);

    // Вызывается после каждого изменения базы — держит JSON-копию всегда актуальной
    private async Task AutoSaveBackupAsync()
    {
        try
        {
            var json = await BuildExportJsonAsync();
            await File.WriteAllTextAsync(GetBackupFilePath(), json);
        }
        catch
        {
            // Автосохранение резервной копии не критично для работы приложения — не прерываем поток
        }
    }

    public async Task<List<Word>> GetWordsAsync()
    {
        await Init();
        return await _connection!.Table<Word>().ToListAsync();
    }

    public async Task<int> SaveWordAsync(Word word)
    {
        await Init();
        int result = word.Id != 0
            ? await _connection!.UpdateAsync(word)
            : await _connection!.InsertAsync(word);

        await AutoSaveBackupAsync();
        return result;
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
                RuleExplanation = dto.Rule?.Trim() ?? string.Empty,
                Category = dto.Category?.Trim() ?? string.Empty,
                Type = WordTypeHelper.FromStringKey(dto.Type),
                Status = LearningStatus.Learning,
                PronunciationNote = dto.PronunciationNote?.Trim() ?? string.Empty
            });
        }

        if (toInsert.Count > 0)
        {
            await _connection!.InsertAllAsync(toInsert);
            await AutoSaveBackupAsync();
        }

        return new ImportResult(toInsert.Count, skipped);
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        await Init();
        var result = await _connection!.DeleteAsync(word);
        await AutoSaveBackupAsync();
        return result;
    }

    // Строит JSON текущего содержимого базы (используется и автосохранением, и ручным экспортом)
    public async Task<string> BuildExportJsonAsync()
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
            Rule = w.RuleExplanation,
            Category = w.Category,
            Type = w.Type.ToString().ToLowerInvariant(),
            PronunciationNote = w.PronunciationNote
        }).ToList();

        return JsonSerializer.Serialize(dtos, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    private static string MakeKey(string korean, string translationRu)
        => $"{korean.Trim().ToLowerInvariant()}|{translationRu.Trim().ToLowerInvariant()}";
}