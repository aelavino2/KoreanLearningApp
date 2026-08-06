using System.Text.Json;

namespace KoreanLearningApp.Infrastructure.Constants
{
    public sealed class ImportJsonOptions
    {
        public JsonSerializerOptions Value { get; }

        public ImportJsonOptions()
        {
            Value = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
        }
    }
}
