using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Import.DTO;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;
using Mapster;
using System.Text.Json;

namespace KoreanLearningApp.Infrastructure.Import;

public class WordImportService(IWordImportRepository repository, JsonSerializerOptions exportOptions) : IWordImportService
{
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
        var dtos = words.Select(w => w.Adapt<WordJsonDto>()).ToList();
        var json = JsonSerializer.Serialize(dtos, exportOptions);
        return Task.FromResult(json);
    }

    private async Task<int> ImportInternalAsync(List<WordJsonDto> dtos)
    {
        if (dtos.Count == 0)
            return 0;

        var existingKeys = await repository.GetExistingKeysAsync();

        var newWords = dtos
            .Select(dto => dto.Adapt<Word>())
            .Where(word => !existingKeys.Contains(BuildKey(word)))
            .ToList();

        if (newWords.Count == 0)
            return 0;

        return await repository.InsertManyAsync(newWords);
    }

    private static string BuildKey(Word word) => $"{word.Korean}_{word.KrDict?.SupNo ?? 0}";
}