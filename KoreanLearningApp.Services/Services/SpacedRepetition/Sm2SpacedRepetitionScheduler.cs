using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services.SpacedRepetition;

public class Sm2SpacedRepetitionScheduler : ISpacedRepetitionScheduler
{
    // Порог интервала, начиная с которого слово считается "выученным" (Known).
    // Значение ориентировочное — 3 недели без ошибок, легко вынести в конфиг позже.
    private const int KnownIntervalThresholdDays = 21;

    public WordProgress CreateInitialProgress(int wordId, DateTime now)
    {
        var initial = Sm2Formula.Initial();

        return new WordProgress
        {
            WordId = wordId,
            LearningStatus = LearningStatusEnum.New,
            EasinessFactor = initial.EasinessFactor,
            IntervalDays = initial.IntervalDays,
            RepetitionCount = initial.RepetitionCount,
            NextReviewDate = now,
            LastReviewedDate = null,
            TotalReviews = 0,
            TotalCorrect = 0,
            LapseCount = 0,
            FirstSeenAt = now
        };
    }

    public WordProgress ApplyReview(WordProgress progress, ReviewRatingEnum rating, DateTime now)
    {
        var result = Sm2Formula.Calculate(
            progress.EasinessFactor,
            progress.IntervalDays,
            progress.RepetitionCount,
            rating);

        progress.EasinessFactor = result.EasinessFactor;
        progress.IntervalDays = result.IntervalDays;
        progress.RepetitionCount = result.RepetitionCount;
        progress.NextReviewDate = now.Date.AddDays(result.IntervalDays);
        progress.LastReviewedDate = now;
        progress.TotalReviews += 1;

        if (result.IsLapse)
            progress.LapseCount += 1;
        else
            progress.TotalCorrect += 1;

        progress.LearningStatus = DetermineStatus(progress, result.IsLapse);

        return progress;
    }

    private static LearningStatusEnum DetermineStatus(WordProgress progress, bool isLapse)
    {
        if (isLapse)
            return LearningStatusEnum.Learning;

        if (progress.IntervalDays >= KnownIntervalThresholdDays)
            return LearningStatusEnum.Known;

        return progress.RepetitionCount <= 1
            ? LearningStatusEnum.Learning
            : LearningStatusEnum.Review;
    }
}