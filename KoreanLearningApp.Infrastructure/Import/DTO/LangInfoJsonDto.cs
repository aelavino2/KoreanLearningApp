using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
