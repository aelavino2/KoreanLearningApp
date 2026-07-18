using KoreanLearningApp.Infrastructure.Serialization.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
