using System.Text.Encodings.Web;
using System.Text.Json;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Abstractions;
using KoreanLearningApp.Infrastructure.Persistence.Constants;
using KoreanLearningApp.Models; // WordImportDto
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoreanLearningApp.Infrastructure.Persistence.Backup;

// Вся логика сериализации/сохранения бэкапа живёт только тут.
public class BackupJsonService : IBackupService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string BuildExportJson(IReadOnlyList<Word> words)
    {
        var dtos = words.Select(ToDto).ToList();
        return JsonSerializer.Serialize(dtos, SerializerOptions);
    }

    public async Task SaveAsync(IReadOnlyList<Word> words)
    {
        try
        {
            var json = BuildExportJson(words);
            await File.WriteAllTextAsync(GetBackupFilePath(), json);
        }
        catch
        {
            // Автосохранение не критично для работы приложения — не прерываем поток
        }
    }

    public static string GetBackupFolder()
    {
#if WINDOWS
        var folder = DbConstants.WindowsDebugFolder;
#else
        var folder = FileSystem.AppDataDirectory;
#endif
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetBackupFilePath() =>
        Path.Combine(GetBackupFolder(), DbConstants.BackupFileName);

    private static WordImportDto ToDto(Word w) => new()
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
    };
}