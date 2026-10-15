using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordPracticeService
{
    /// <summary>
    /// Собирает квиз: сначала сохранённые слова (если включено), затем слова к повторению, затем новые.
    /// </summary>
    Task<List<QuizQuestion>> CreateQuizAsync(PracticeSessionOptions options);

    Task SubmitReviewAsync(int wordId, ReviewRatingEnum rating);
}
