using System.Text.Json.Serialization;

namespace KoreanLearningApp.Infrastructure.Import.DTO;

public class TagJsonDto
{
    [JsonPropertyName("name_ko")]
    public string NameKo { get; set; } = string.Empty;

    [JsonPropertyName("name_ru")]
    public string NameRu { get; set; } = string.Empty;
}
