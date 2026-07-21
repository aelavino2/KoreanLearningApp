using System.Text.Json.Serialization;

namespace KoreanLearningApp.Infrastructure.Import.DTO
{
    public abstract class KrDictJsonDto
    {
        [JsonPropertyName("target_code")]
        public string TargetCode { get; set; } = string.Empty;

        [JsonPropertyName("word")]
        public string Word { get; set; } = string.Empty;

        [JsonPropertyName("sup_no")]
        public int SupNo { get; set; }

        [JsonPropertyName("pos")]
        public string Pos { get; set; } = string.Empty;

        [JsonPropertyName("pronunciation")]
        public string Pronunciation { get; set; } = string.Empty;

        [JsonPropertyName("word_grade")]
        public string WordGrade { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("senses")]
        public List<SenseJsonDto> Senses { get; set; } = new();

        [JsonPropertyName("audio")]
        public AudioJsonDto? Audio { get; set; }
    }
}
