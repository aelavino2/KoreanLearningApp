using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Services.Practice;

namespace KoreanLearningApp.Tests.Practice;

public class SpacedRepetitionTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 9, 0, 0);

    private readonly Sm2SpacedRepetitionScheduler _scheduler = new();

    [Fact]
    public void ApplyReview_GoodAnswersInRow_IntervalGrowsAndWordBecomesKnown()
    {
        var progress = _scheduler.CreateInitialProgress(wordId: 1, Now);

        var intervals = Enumerable.Range(0, 4)
            .Select(_ =>
            {
                _scheduler.ApplyReview(progress, ReviewRatingEnum.Good, Now);
                return progress.IntervalDays;
            })
            .ToList();

        Assert.Equal([1, 6, 17, 49], intervals);
        Assert.Equal(LearningStatusEnum.Known, progress.LearningStatus);
        Assert.Equal(Now.Date.AddDays(49), progress.NextReviewDate);
    }

    [Fact]
    public void ApplyReview_ForgotAfterProgress_ResetsRepetitionsAndCountsLapse()
    {
        var progress = _scheduler.CreateInitialProgress(wordId: 1, Now);
        _scheduler.ApplyReview(progress, ReviewRatingEnum.Good, Now);
        _scheduler.ApplyReview(progress, ReviewRatingEnum.Good, Now);

        _scheduler.ApplyReview(progress, ReviewRatingEnum.Forgot, Now);

        Assert.Equal((0, 1, 1), (progress.RepetitionCount, progress.IntervalDays, progress.LapseCount));
        Assert.Equal(LearningStatusEnum.Learning, progress.LearningStatus);
    }

    [Fact]
    public void Calculate_RepeatedForgot_EasinessNeverDropsBelowMinimum()
    {
        var easiness = WordProgress.StartingEasinessFactor;

        for (var i = 0; i < 20; i++)
            easiness = Sm2Formula.Calculate(easiness, 1, 0, ReviewRatingEnum.Forgot).EasinessFactor;

        Assert.Equal(1.3, easiness, precision: 6);
    }

    [Theory]
    [InlineData(false, 10, ReviewRatingEnum.Forgot)]
    [InlineData(true, 5, ReviewRatingEnum.Good)]
    [InlineData(true, 4, ReviewRatingEnum.Hard)]
    public void DetermineRating_AnswerAndTimeLeft_MapsToRating(bool wasCorrect, int secondsLeft, ReviewRatingEnum expected)
    {
        var rating = new QuizScoringService().DetermineRating(wasCorrect, secondsLeft, questionSeconds: 10);

        Assert.Equal(expected, rating);
    }
}
