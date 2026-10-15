using System.Text.Json;
using System.Text.Json.Serialization;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Infrastructure.Mapster;

public sealed class NiklLevelJsonConverter : JsonConverter<NiklLevelEnum>
{
    public override NiklLevelEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => NiklLevelMapper.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, NiklLevelEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToKorean());
}

public sealed class TopikLevelJsonConverter : JsonConverter<TopikLevelEnum>
{
    public override TopikLevelEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TopikLevelMapper.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, TopikLevelEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToCode());
}