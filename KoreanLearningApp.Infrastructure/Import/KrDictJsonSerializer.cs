using KoreanLearningApp.Infrastructure.Import.DTO;
using System.Text.Json;

namespace KoreanLearningApp.Infrastructure.Import;

internal static class KrDictJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    public static List<WordJsonDto> DeserializeArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<WordJsonDto>();

        return JsonSerializer.Deserialize<List<WordJsonDto>>(json, Options)
               ?? new List<WordJsonDto>();
    }

    public static async Task<List<WordJsonDto>> DeserializeFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Import file not found: {filePath}", filePath);

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<WordJsonDto>>(stream, Options)
               ?? new List<WordJsonDto>();
    }
}