using System.Text.Json.Serialization;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Infrastructure.Mapster;

namespace KoreanLearningApp.Infrastructure.Import.DTO;

public class WordJsonDto
{
    [JsonPropertyName("rank")]
    public string Rank { get; set; } = string.Empty;

    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("part_of_speech")]
    [JsonConverter(typeof(PartOfSpeechJsonConverter))]
    public PartOfSpeechEnum PartOfSpeech { get; set; }

    [JsonPropertyName("hanja")]
    public string Hanja { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonPropertyName("nikl_level")]
    public string NiklLevel { get; set; } = string.Empty;

    [JsonPropertyName("topik_level")]
    public string TopikLevel { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("krdict")]
    public KrDictJsonDto? KrDict { get; set; }

    [JsonPropertyName("_idx")]
    public int Idx { get; set; }
}