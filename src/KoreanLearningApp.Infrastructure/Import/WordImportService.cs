using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Import.Abstraction;
using KoreanLearningApp.Infrastructure.Import.DTO;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;
using Mapster;

namespace KoreanLearningApp.Infrastructure.Import;

public sealed class WordImportService(IWordImportRepository repository,
    IKrDictJsonSerializer krDictSerializer) : IWordImportService
{
    public async Task<int> ImportFromFileAsync(string filePath)
    {
        var dtos = await krDictSerializer.DeserializeFileAsync(filePath);
        return await ImportInternalAsync(dtos);
    }

    public async Task<int> ImportFromJsonAsync(string json)
    {
        var dtos = krDictSerializer.DeserializeArray(json);
        return await ImportInternalAsync(dtos);
    }

    private async Task<int> ImportInternalAsync(List<WordJsonDto> dtos)
    {
        if (dtos.Count == 0) return 0;

        var existingKeys = await repository.GetExistingKeysAsync();

        var newWords = dtos
            .Select(dto => dto.Adapt<Word>())
            .Where(word => !existingKeys.Contains(BuildKey(word)))
            .ToList();

        return newWords.Count == 0 ? 0 : await repository.InsertManyAsync(newWords);
    }

    private static string BuildKey(Word word) =>
        $"{word.Korean}_{word.KrDict?.SupNo ?? 0}";
}