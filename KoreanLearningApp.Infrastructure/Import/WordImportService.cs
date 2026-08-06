using System.Text.Json;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Import.DTO;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Infrastructure.Import;

public class WordImportService(IWordImportRepository repository) : IWordImportService
{
    private static readonly JsonSerializerOptions ExportOptions = new()
    {
        WriteIndented = true,
    };

    public async Task<int> ImportFromFileAsync(string filePath)
    {
        var dtos = await KrDictJsonSerializer.DeserializeFileAsync(filePath);
        return await ImportInternalAsync(dtos);
    }

    public async Task<int> ImportFromJsonAsync(string json)
    {
        var dtos = KrDictJsonSerializer.DeserializeArray(json);
        return await ImportInternalAsync(dtos);
    }

    public Task<string> ExportToJsonAsync(IEnumerable<Word> words)
    {
        var dtos = words.Select(w => w.ToDto()).ToList();
        var json = JsonSerializer.Serialize(dtos, ExportOptions);
        return Task.FromResult(json);
    }

    private async Task<int> ImportInternalAsync(List<WordJsonDto> dtos)
    {
        if (dtos.Count == 0)
            return 0;

        var existingKeys = await repository.GetExistingKeysAsync();

        var newWords = dtos
            .Select(dto => dto.ToDomain())
            .Where(word => !existingKeys.Contains(BuildKey(word)))
            .ToList();

        if (newWords.Count == 0)
            return 0;

        return await repository.InsertManyAsync(newWords);
    }

    private static string BuildKey(Word word) =>
        $"{word.Korean}_{word.KrDict?.SupNo ?? 0}";
}