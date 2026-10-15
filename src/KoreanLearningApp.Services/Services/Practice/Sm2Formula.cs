using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Services.Practice;

public readonly record struct Sm2Result(double EasinessFactor, int IntervalDays, int RepetitionCount, bool IsLapse);

/// <summary>
/// Алгоритм интервальных повторений SuperMemo SM-2.
/// </summary>
public static class Sm2Formula
{
    private const double MinEasinessFactor = 1.3;
    private const int MinPassingQuality = 3;
    private const int FirstIntervalDays = 1;
    private const int SecondIntervalDays = 6;

    public static Sm2Result Calculate(double easinessFactor, int intervalDays, int repetitionCount, ReviewRatingEnum rating)
    {
        var quality = ToQuality(rating);
        var newEasinessFactor = Math.Max(MinEasinessFactor,
            easinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02)));

        if (quality < MinPassingQuality)
            return new Sm2Result(newEasinessFactor, FirstIntervalDays, RepetitionCount: 0, IsLapse: true);

        var newRepetitionCount = repetitionCount + 1;
        var newIntervalDays = newRepetitionCount switch
        {
            1 => FirstIntervalDays,
            2 => SecondIntervalDays,
            _ => (int)Math.Round(intervalDays * newEasinessFactor, MidpointRounding.AwayFromZero)
        };

        return new Sm2Result(newEasinessFactor, newIntervalDays, newRepetitionCount, IsLapse: false);
    }

    private static int ToQuality(ReviewRatingEnum rating) => rating switch
    {
        ReviewRatingEnum.Forgot => 2,
        ReviewRatingEnum.Hard => 3,
        ReviewRatingEnum.Good => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(rating), rating, null)
    };
}
