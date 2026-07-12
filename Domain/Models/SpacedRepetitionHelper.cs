using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Models;

public static class SpacedRepetitionHelper
{
    private const int MinBox = 1;
    private const int MaxBox = 5;

    private static readonly Dictionary<int, int> IntervalDays = new()
    {
        { 1, 0 },
        { 2, 1 },
        { 3, 3 },
        { 4, 7 },
        { 5, 14 }
    };

    public static void ApplyAnswer(Word word, bool wasCorrect)
    {
        if (wasCorrect)
        {
            word.LeitnerBox = Math.Min(word.LeitnerBox + 1, MaxBox);
        }
        else
        {
            word.LeitnerBox = MinBox;
        }

        var intervalDays = IntervalDays.TryGetValue(word.LeitnerBox, out var days) ? days : 0;
        word.LastReviewedAt = DateTime.UtcNow;
        word.NextReviewAt = DateTime.UtcNow.AddDays(intervalDays);

        word.Status = word.LeitnerBox >= MaxBox ? LearningStatus.Learned : LearningStatus.Learning;
    }

    public static bool IsDue(Word word) => word.NextReviewAt <= DateTime.UtcNow;

    // Тихий индикатор прогресса: заполненные и пустые звёзды, без слов и цифр
    public static string ProgressStars(int box)
    {
        var clamped = Math.Clamp(box, MinBox, MaxBox);
        return new string('★', clamped) + new string('☆', MaxBox - clamped);
    }
}