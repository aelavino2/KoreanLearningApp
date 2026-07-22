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

        json = json.Trim();

        try
        {
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.ValueKind switch
            {
                JsonValueKind.Array =>
                    JsonSerializer.Deserialize<List<WordJsonDto>>(json, Options) ?? new List<WordJsonDto>(),

                JsonValueKind.Object =>
                    JsonSerializer.Deserialize<WordJsonDto>(json, Options) is { } single
                        ? new List<WordJsonDto> { single }
                        : new List<WordJsonDto>(),

                _ => new List<WordJsonDto>()
            };
        }
        catch (JsonException)
        {
            return DeserializeConcatenatedObjects(json);
        }
    }

    public static async Task<List<WordJsonDto>> DeserializeFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Import file not found: {filePath}", filePath);

        var json = await File.ReadAllTextAsync(filePath);
        return DeserializeArray(json);
    }

    private static List<WordJsonDto> DeserializeConcatenatedObjects(string json)
    {
        var result = new List<WordJsonDto>();

        foreach (var chunk in SplitTopLevelJsonValues(json))
        {
            var dto = JsonSerializer.Deserialize<WordJsonDto>(chunk, Options);
            if (dto is not null)
                result.Add(dto);
        }

        return result;
    }
    
    private static IEnumerable<string> SplitTopLevelJsonValues(string json)
    {
        int depth = 0;
        int start = -1;
        bool inString = false;
        bool escape = false;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (inString)
            {
                if (escape) escape = false;
                else if (c == '\\') escape = true;
                else if (c == '"') inString = false;
                continue;
            }

            switch (c)
            {
                case '"':
                    inString = true;
                    break;
                case '{':
                case '[':
                    if (depth == 0) start = i;
                    depth++;
                    break;
                case '}':
                case ']':
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        yield return json.Substring(start, i - start + 1);
                        start = -1;
                    }
                    break;
            }
        }
    }
}