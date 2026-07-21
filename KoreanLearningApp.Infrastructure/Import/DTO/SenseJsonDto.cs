using System.Text.Json.Serialization;

namespace KoreanLearningApp.Infrastructure.Import.DTO
{
    public class SenseJsonDto
    {
        [JsonPropertyName("definition_ko")]
        public string DefinitionKo { get; set; } = string.Empty;

        [JsonPropertyName("en")]
        public LangInfoJsonDto? En { get; set; }

        [JsonPropertyName("ru")]
        public LangInfoJsonDto? Ru { get; set; }
    }
}
