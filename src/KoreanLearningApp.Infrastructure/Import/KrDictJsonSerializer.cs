using KoreanLearningApp.Infrastructure.Constants;
using KoreanLearningApp.Infrastructure.Import.Abstraction;
using KoreanLearningApp.Infrastructure.Import.DTO;
using System.Text.Json;
using System.Text;
namespace KoreanLearningApp.Infrastructure.Import;

public class KrDictJsonSerializer(ImportJsonOptions options) : IKrDictJsonSerializer
{
    public List<WordJsonDto> DeserializeArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<WordJsonDto>();


        var readerOptions = new JsonReaderOptions
        {
            AllowMultipleValues = true
        };  

        var bytes = Encoding.UTF8.GetBytes(json.Trim());
        var reader = new Utf8JsonReader(bytes, readerOptions);

        var result = new List<WordJsonDto>();

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.StartObject)
            {
                var dto = JsonSerializer.Deserialize<WordJsonDto>(ref reader, options.Value);
                if (dto is not null)
                    result.Add(dto);
            }
        }

        return result;
    }

    public async Task<List<WordJsonDto>> DeserializeFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Import file not found: {filePath}", filePath);

        var json = await File.ReadAllTextAsync(filePath);
        return DeserializeArray(json);
    }
}