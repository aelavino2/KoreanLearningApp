using System.Text.Json;
using System.Text.Json.Serialization;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Infrastructure.Mapster;

public sealed class PartOfSpeechJsonConverter : JsonConverter<PartOfSpeechEnum>
{
    public override PartOfSpeechEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return PartOfSpeechMapper.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, PartOfSpeechEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToKorean());
    }
}
