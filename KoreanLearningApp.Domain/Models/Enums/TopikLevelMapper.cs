
namespace KoreanLearningApp.Domain.Models.Enums;

public static class TopikLevelMapper
{
    public static TopikLevelEnum Parse(string? level)
    {
        return level?.Trim() switch
        {
            "A" => TopikLevelEnum.A,
            "B" => TopikLevelEnum.B,
            "C" => TopikLevelEnum.C,
            _ => TopikLevelEnum.Empty
        };
    }

    public static string ToCode(this TopikLevelEnum level) => level switch
    {
        TopikLevelEnum.A => "A",
        TopikLevelEnum.B => "B",
        TopikLevelEnum.C => "C",
        _ => string.Empty
    };
}