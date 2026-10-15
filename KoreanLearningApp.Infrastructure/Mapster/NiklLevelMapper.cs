using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Infrastructure.Mapster;

public static class NiklLevelMapper
{
    public static NiklLevelEnum Parse(string? level)
    {
        return level?.Trim() switch
        {
            "초급" => NiklLevelEnum.Beginner,
            "중급" => NiklLevelEnum.Intermediate,
            "고급" => NiklLevelEnum.Advanced,
            _ => NiklLevelEnum.Empty
        };
    }

    public static string ToKorean(this NiklLevelEnum level) => level switch
    {
        NiklLevelEnum.Beginner => "초급",
        NiklLevelEnum.Intermediate => "중급",
        NiklLevelEnum.Advanced => "고급",
        _ => string.Empty
    };

    public static string ToRussian(this NiklLevelEnum level) => level switch
    {
        NiklLevelEnum.Beginner => "начальный",
        NiklLevelEnum.Intermediate => "средний",
        NiklLevelEnum.Advanced => "продвинутый",
        _ => "—"
    };
}