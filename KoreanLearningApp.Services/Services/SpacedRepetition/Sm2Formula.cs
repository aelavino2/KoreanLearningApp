using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Services.SpacedRepetition;

public readonly record struct Sm2Result(double EasinessFactor, int IntervalDays, int RepetitionCount, bool IsLapse);

public static class Sm2Formula
{
    private const double MinEasinessFactor = 1.3;
    private const double StartingEasinessFactor = 2.5;

    private static int MapRatingToQuality(ReviewRatingEnum rating) => rating switch
    {
        ReviewRatingEnum.Forgot => 2,
        ReviewRatingEnum.Hard => 3,
        ReviewRatingEnum.Good => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(rating), rating, "Неизвестная оценка ответа")
    };

    public static Sm2Result Initial() => new(EasinessFactor: StartingEasinessFactor, IntervalDays: 0,
        RepetitionCount: 0, IsLapse: false);

    public static Sm2Result Calculate(double currentEasinessFactor, int currentIntervalDays,
        int currentRepetitionCount, ReviewRatingEnum rating)
    {
        var q = MapRatingToQuality(rating);

        var newEasinessFactor = currentEasinessFactor + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
        if (newEasinessFactor < MinEasinessFactor)
            newEasinessFactor = MinEasinessFactor;

        if (q < 3)
        {
            return new Sm2Result(EasinessFactor: newEasinessFactor, IntervalDays: 1,
                RepetitionCount: 0, IsLapse: true);
        }

        var newRepetitionCount = currentRepetitionCount + 1;
        var newIntervalDays = newRepetitionCount switch
        {
            1 => 1,
            2 => 6,
            _ => (int)Math.Round(currentIntervalDays * newEasinessFactor, MidpointRounding.AwayFromZero)
        };

        return new Sm2Result(EasinessFactor: newEasinessFactor, IntervalDays: newIntervalDays,
            RepetitionCount: newRepetitionCount, IsLapse: false);
    }
}