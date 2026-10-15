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
    public async Task<WordImportResult> ImportFromFileAsync(string filePath)
    {
        var dtos = await krDictSerializer.DeserializeFileAsync(filePath);
        return await ImportInternalAsync(dtos);
    }

    public async Task<WordImportResult> ImportFromJsonAsync(string json)
    {
        var dtos = krDictSerializer.DeserializeArray(json);
        return await ImportInternalAsync(dtos);
    }

    private async Task<WordImportResult> ImportInternalAsync(List<WordJsonDto> dtos)
    {
        if (dtos.Count == 0)
            return new WordImportResult(0, 0);

        var existingKeys = await repository.GetExistingKeysAsync();
        var words = dtos.Select(dto => dto.Adapt<Word>()).ToList();

        var newWords = words.Where(word => !existingKeys.Contains(BuildKey(word))).ToList();
        var tagsOfExistingWords = words
            .Where(word => word.Tags.Count > 0 && existingKeys.Contains(BuildKey(word)))
            .GroupBy(BuildKey)
            .ToDictionary(g => g.Key, g => g.SelectMany(word => word.Tags).ToList());

        var added = newWords.Count == 0 ? 0 : await repository.InsertManyAsync(newWords);
        var tagged = await repository.AddTagsAsync(tagsOfExistingWords);

        return new WordImportResult(added, tagged);
    }

    private static string BuildKey(Word word) =>
        $"{word.Korean}_{word.KrDict?.SupNo ?? 0}";
}