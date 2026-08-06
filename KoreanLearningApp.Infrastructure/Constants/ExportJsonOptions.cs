using System.Text.Json;
using System.Text.Json.Serialization;

namespace KoreanLearningApp.Infrastructure.Constants
{
    public sealed class ExportJsonOptions
    {
        public JsonSerializerOptions Value { get; }

        public ExportJsonOptions()
        {
            Value = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
        }
    }
}
