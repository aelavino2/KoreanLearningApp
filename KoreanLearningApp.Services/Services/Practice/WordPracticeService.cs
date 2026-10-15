using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services.Practice;

public sealed class WordPracticeService(
    IWordRepository wordRepository,
    IWordProgressRepository progressRepository,
    IReviewLogRepository reviewLogRepository,
    ISavedWordRepository savedWordRepository,
    ISpacedRepetitionScheduler scheduler,
    IQuizQuestionFactory questionFactory,
    TimeProvider timeProvider) : IWordPracticeService
{
    private const int DistractorPoolSize = 100;

    public async Task<List<QuizQuestion>> CreateQuizAsync(PracticeSessionOptions options)
    {
        var cards = await SelectCardsAsync(options);
        if (cards.Count == 0)
            return [];

        var distractorPool = await wordRepository.GetRandomAsync(DistractorPoolSize);
        return questionFactory.Create(cards, options.QuizDirection, distractorPool);
    }

    public async Task SubmitReviewAsync(int wordId, ReviewRatingEnum rating)
    {
        var now = timeProvider.GetLocalNow().DateTime;
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

    private async Task<List<PracticeCard>> SelectCardsAsync(PracticeSessionOptions options)
    {
        var wordCount = Math.Max(1, options.WordCount);
        var cards = new List<PracticeCard>();

        if (options.IncludeSavedWords)
            cards.AddRange(await GetSavedCardsAsync(wordCount, options.TopikLevels));

        if (cards.Count < wordCount)
            cards.AddRange(await GetDueCardsAsync(wordCount - cards.Count, options.TopikLevels, cards));

        if (cards.Count < wordCount)
            cards.AddRange(await GetNewCardsAsync(wordCount - cards.Count, options.TopikLevels, cards));

        return cards;
    }

    private async Task<List<PracticeCard>> GetSavedCardsAsync(int limit, IReadOnlyCollection<string> topikLevels)
    {
        var savedIds = await savedWordRepository.GetSavedWordIdsAsync();
        var words = (await wordRepository.GetByIdsAsync(savedIds))
            .Where(w => MatchesTopikLevel(w, topikLevels))
            .Take(limit)
            .ToList();

        if (words.Count == 0)
            return [];

        var progressByWordId = (await progressRepository.GetByWordIdsAsync(words.Select(w => w.Id).ToList()))
            .ToDictionary(p => p.WordId);

        return words
            .Select(w => new PracticeCard { Word = w, Progress = progressByWordId.GetValueOrDefault(w.Id) })
            .ToList();
    }

    private async Task<List<PracticeCard>> GetDueCardsAsync(int limit, IReadOnlyCollection<string> topikLevels,
        IReadOnlyCollection<PracticeCard> alreadySelected)
    {
        var now = timeProvider.GetLocalNow().DateTime;
        var selectedIds = alreadySelected.Select(c => c.Word.Id).ToHashSet();

        var dueProgress = (await progressRepository.GetDueAsync(now, limit + selectedIds.Count, topikLevels))
            .Where(p => !selectedIds.Contains(p.WordId))
            .Take(limit)
            .ToList();

        var wordsById = (await wordRepository.GetByIdsAsync(dueProgress.Select(p => p.WordId).ToList()))
            .ToDictionary(w => w.Id);

        return dueProgress
            .Where(p => wordsById.ContainsKey(p.WordId))
            .Select(p => new PracticeCard { Word = wordsById[p.WordId], Progress = p })
            .ToList();
    }

    private async Task<List<PracticeCard>> GetNewCardsAsync(int limit, IReadOnlyCollection<string> topikLevels,
        IReadOnlyCollection<PracticeCard> alreadySelected)
    {
        var selectedIds = alreadySelected.Select(c => c.Word.Id).ToHashSet();

        var newIds = (await progressRepository.GetNewWordIdsAsync(limit + selectedIds.Count, topikLevels))
            .Where(id => !selectedIds.Contains(id))
            .Take(limit)
            .ToList();

        return (await wordRepository.GetByIdsAsync(newIds))
            .Select(w => new PracticeCard { Word = w })
            .ToList();
    }

    private static bool MatchesTopikLevel(Word word, IReadOnlyCollection<string> topikLevels) =>
        topikLevels.Count == 0 || topikLevels.Contains(word.TopikLevel.Trim());
}
