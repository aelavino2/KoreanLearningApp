using System.Text.Json.Serialization;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Infrastructure.Mapster;

namespace KoreanLearningApp.Infrastructure.Import.DTO
{
    public class KrDictJsonDto
    {
        [JsonPropertyName("target_code")]
        public string TargetCode { get; set; } = string.Empty;

        [JsonPropertyName("word")]
        public string Word { get; set; } = string.Empty;

        [JsonPropertyName("sup_no")]
        public int SupNo { get; set; }

        [JsonPropertyName("pos")]
        [JsonConverter(typeof(PartOfSpeechJsonConverter))]
        public PartOfSpeechEnum Pos { get; set; }

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
