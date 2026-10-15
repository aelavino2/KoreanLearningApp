using System.Text.Json;
using System.Text.Json.Serialization;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Infrastructure.Mapster;

/// <summary>
/// Читает часть речи по корейскому названию ("명사") или по имени enum ("Noun"); пишет корейское название.
/// </summary>
public sealed class PartOfSpeechJsonConverter : JsonConverter<PartOfSpeechEnum>
{
    public override PartOfSpeechEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        var pos = PartOfSpeechMapper.Parse(value);
        if (pos != PartOfSpeechEnum.Empty)
            return pos;

        return Enum.TryParse(value, ignoreCase: true, out PartOfSpeechEnum byName) && Enum.IsDefined(byName)
            ? byName
            : PartOfSpeechEnum.Empty;
    }

    public override void Write(Utf8JsonWriter writer, PartOfSpeechEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToKorean());
    }
}
