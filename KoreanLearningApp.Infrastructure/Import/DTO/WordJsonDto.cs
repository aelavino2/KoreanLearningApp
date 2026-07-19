using System.Text.Json.Serialization;

namespace KoreanLearningApp.Infrastructure.Import.DTO;

public abstract class WordJsonDto
{
    [JsonPropertyName("rank")]
    public string Rank { get; set; } = string.Empty;

    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("part_of_speech")]
    public string PartOfSpeech { get; set; } = string.Empty;

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