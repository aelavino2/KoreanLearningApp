using System.Text.Json.Serialization;

namespace KoreanLearningApp.Infrastructure.Import.DTO
{
    public class AudioJsonDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("file")]
        public string File { get; set; } = string.Empty;
    }
}
