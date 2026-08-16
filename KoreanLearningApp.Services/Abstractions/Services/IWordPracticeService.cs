using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordPracticeService
{
    Task<List<PracticeCard>> GetPracticeSessionAsync(PracticeSessionOptions options);
    Task SubmitReviewAsync(int wordId, ReviewRatingEnum rating);
}