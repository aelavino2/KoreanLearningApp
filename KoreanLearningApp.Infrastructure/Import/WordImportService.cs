using KoreanLearningApp.Infrastructure.Import.DTO;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Infrastructure.Import;

public class WordImportService : IWordImportService
{
    private readonly IWordImportRepository _repository;

    public WordImportService(IWordImportRepository repository)
    {
        _repository = repository;
    }

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

    private async Task<int> ImportInternalAsync(List<WordJsonDto> dtos)
    {
        if (dtos.Count == 0)
            return 0;

        var existingKeys = await _repository.GetExistingKeysAsync();

        var newWords = dtos
            .Select(dto => dto.ToDomain())
            .Where(word => !existingKeys.Contains(BuildKey(word.Korean, word.SupNo)))
            .ToList();

        if (newWords.Count == 0)
            return 0;

        return await _repository.InsertManyAsync(newWords);
    }

    private static string BuildKey(string korean, int supNo) => $"{korean}_{supNo}";
}