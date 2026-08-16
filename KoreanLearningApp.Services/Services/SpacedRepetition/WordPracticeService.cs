using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services.SpacedRepetition;

/// <summary>
/// Связывает вместе три вещи, которые по отдельности ничего не знают друг о друге:
/// репозиторий прогресса, репозиторий словаря и планировщик SM-2. Это единственное
/// место в приложении, где формируется сессия практики и обрабатывается ответ пользователя.
/// </summary>
public class WordPracticeService(
    IWordProgressRepository progressRepository,
    IReviewLogRepository reviewLogRepository,
    ISpacedRepetitionScheduler scheduler,
    IWordRepository wordRepository) : IWordPracticeService
{
    public async Task<List<PracticeCard>> GetPracticeSessionAsync(int dueLimit = 20, int newLimit = 5)
    {
        var now = DateTime.Now;

        var dueProgress = await progressRepository.GetDueAsync(now, dueLimit);
        var newWordIds = await progressRepository.GetNewWordIdsAsync(newLimit);

        var allWordIds = dueProgress
            .Select(p => p.WordId)
            .Concat(newWordIds)
            .Distinct()
            .ToList();

        if (allWordIds.Count == 0)
            return new List<PracticeCard>();

        var words = await wordRepository.GetByIdsAsync(allWordIds);
        var wordsById = words.ToDictionary(w => w.Id);

        var cards = new List<PracticeCard>();

        // Сначала due-слова (уже отсортированы по NextReviewDate в репозитории) —
        // повторение существующих слов приоритетнее показа новых.
        foreach (var progress in dueProgress)
        {
            if (wordsById.TryGetValue(progress.WordId, out var word))
                cards.Add(new PracticeCard { Word = word, Progress = progress });
        }

        // Затем новые слова (уже отсортированы по Rank в репозитории)
        foreach (var wordId in newWordIds)
        {
            if (wordsById.TryGetValue(wordId, out var word))
                cards.Add(new PracticeCard { Word = word, Progress = null });
        }

        return cards;
    }

    public async Task SubmitReviewAsync(int wordId, ReviewRatingEnum rating)
    {
        var now = DateTime.Now;

        var progress = await progressRepository.GetByWordIdAsync(wordId)
                        ?? scheduler.CreateInitialProgress(wordId, now);

        var intervalBefore = progress.IntervalDays;
        var easinessBefore = progress.EasinessFactor;

        scheduler.ApplyReview(progress, rating, now);

        await progressRepository.UpsertAsync(progress);

        await reviewLogRepository.AddAsync(new ReviewLog
        {
            WordId = wordId,
            ReviewedAt = now,
            Rating = rating,
            IntervalBefore = intervalBefore,
            IntervalAfter = progress.IntervalDays,
            EasinessBefore = easinessBefore,
            EasinessAfter = progress.EasinessFactor
        });
    }
}