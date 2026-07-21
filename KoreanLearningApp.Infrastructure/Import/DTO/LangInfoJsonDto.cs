using System.Text.Json.Serialization;

namespace KoreanLearningApp.Infrastructure.Import.DTO
{
    public class LangInfoJsonDto
    {
        [JsonPropertyName("word")]
        public string Word { get; set; } = string.Empty;

        [JsonPropertyName("definition")]
        public string Definition { get; set; } = string.Empty;
    }
}
