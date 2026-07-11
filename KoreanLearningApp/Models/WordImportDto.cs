using System.Text.Json.Serialization;
namespace KoreanLearningApp.Models;

public class WordImportDto
{
    [JsonPropertyName("korean")]
    public string Korean { get; set; } = string.Empty;

    [JsonPropertyName("transcriptionRu")]
    public string TranscriptionRu { get; set; } = string.Empty;

    [JsonPropertyName("transcriptionEn")]
    public string TranscriptionEn { get; set; } = string.Empty;

    [JsonPropertyName("translationRu")]
    public string TranslationRu { get; set; } = string.Empty;

    [JsonPropertyName("translationEn")]
    public string TranslationEn { get; set; } = string.Empty;

    [JsonPropertyName("rule")]
    public string Rule { get; set; } = string.Empty;
}