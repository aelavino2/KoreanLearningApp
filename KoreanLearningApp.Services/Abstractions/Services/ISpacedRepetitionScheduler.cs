using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface ISpacedRepetitionScheduler
{
    WordProgress CreateInitialProgress(int wordId, DateTime now);
    WordProgress ApplyReview(WordProgress progress, ReviewRatingEnum rating, DateTime now);
}