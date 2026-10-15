using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services.Practice;

public sealed class QuizScoringService : IQuizScoringService
{
    private const int BaseCorrectPoints = 10;

    public ReviewRatingEnum DetermineRating(bool wasCorrect, int secondsLeft, int questionSeconds)
    {
        if (!wasCorrect)
            return ReviewRatingEnum.Forgot;

        return secondsLeft >= questionSeconds / 2 ? ReviewRatingEnum.Good : ReviewRatingEnum.Hard;
    }

    public int CalculateScore(bool wasCorrect, int secondsLeft) =>
        wasCorrect ? BaseCorrectPoints + secondsLeft : 0;
}
