using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services.Practice;

public sealed class Sm2SpacedRepetitionScheduler : ISpacedRepetitionScheduler
{
    private const int KnownIntervalThresholdDays = 21;

    public WordProgress CreateInitialProgress(int wordId, DateTime now) => new()
    {
        WordId = wordId,
        NextReviewDate = now,
        FirstSeenAt = now
    };

    public void ApplyReview(WordProgress progress, ReviewRatingEnum rating, DateTime now)
    {
        var result = Sm2Formula.Calculate(progress.EasinessFactor, progress.IntervalDays, progress.RepetitionCount, rating);

        progress.EasinessFactor = result.EasinessFactor;
        progress.IntervalDays = result.IntervalDays;
        progress.RepetitionCount = result.RepetitionCount;
        progress.NextReviewDate = now.Date.AddDays(result.IntervalDays);
        progress.LastReviewedDate = now;
        progress.TotalReviews++;

        if (result.IsLapse)
            progress.LapseCount++;
        else
            progress.TotalCorrect++;

        progress.LearningStatus = DetermineStatus(result);
    }

    private static LearningStatusEnum DetermineStatus(Sm2Result result)
    {
        if (result.IsLapse)
            return LearningStatusEnum.Learning;

        if (result.IntervalDays >= KnownIntervalThresholdDays)
            return LearningStatusEnum.Known;

        return result.RepetitionCount <= 1 ? LearningStatusEnum.Learning : LearningStatusEnum.Review;
    }
}
