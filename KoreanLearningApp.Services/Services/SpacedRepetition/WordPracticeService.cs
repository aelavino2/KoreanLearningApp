using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services.SpacedRepetition;

public class WordPracticeService(
    IWordProgressRepository progressRepository,
    IReviewLogRepository reviewLogRepository,
    ISpacedRepetitionScheduler scheduler,
    IWordRepository wordRepository,
    ISavedWordRepository savedWordRepository) : IWordPracticeService
{
    public async Task<List<PracticeCard>> GetPracticeSessionAsync(PracticeSessionOptions options)
    {
        var now = DateTime.Now;
        var wordCount = Math.Max(1, options.WordCount);
        var topikLevels = NormalizeLevels(options.TopikLevels);

        var cards = new List<PracticeCard>();
        var usedWordIds = new HashSet<int>();

        if (options.IncludeSavedWords)
            await AddSavedWordsAsync(cards, usedWordIds, wordCount, topikLevels);


        var remaining = wordCount - cards.Count;
        if (remaining > 0)
        {
            await AddDueWordsAsync(cards, usedWordIds, now, remaining, topikLevels);
        }

        remaining = wordCount - cards.Count;
        if (remaining > 0)
        {
            await AddNewWordsAsync(cards, usedWordIds, remaining, topikLevels);
        }

        return cards;
    }

    private async Task AddSavedWordsAsync(List<PracticeCard> cards, HashSet<int> usedWordIds, int wordCount, List<string> topikLevels)
    {
        var savedIds = await savedWordRepository.GetSavedWordIdsAsync();
        if (savedIds.Count == 0)
            return;

        var savedWords = await wordRepository.GetByIdsAsync(savedIds);

        var wordsById = savedWords.ToDictionary(w => w.Id);
        var orderedFiltered = savedIds
            .Where(id => wordsById.ContainsKey(id))
            .Select(id => wordsById[id])
            .Where(w => MatchesTopik(w.TopikLevel, topikLevels))
            .Take(wordCount)
            .ToList();

        if (orderedFiltered.Count == 0)
            return;

        var progressList = await progressRepository.GetByWordIdsAsync(orderedFiltered.Select(w => w.Id).ToList());
        var progressByWordId = progressList.ToDictionary(p => p.WordId);

        foreach (var word in orderedFiltered)
        {
            cards.Add(new PracticeCard
            {
                Word = word,
                Progress = progressByWordId.GetValueOrDefault(word.Id)
            });
            usedWordIds.Add(word.Id);
        }
    }

    private async Task AddDueWordsAsync(List<PracticeCard> cards, HashSet<int> usedWordIds, DateTime now, int limit, List<string> topikLevels)
    {
        var dueProgress = await progressRepository.GetDueAsync(now, limit, topikLevels);
        var freshDue = dueProgress.Where(p => !usedWordIds.Contains(p.WordId)).ToList();
        if (freshDue.Count == 0)
            return;

        var dueWords = await wordRepository.GetByIdsAsync(freshDue.Select(p => p.WordId).ToList());
        var wordsById = dueWords.ToDictionary(w => w.Id);

        foreach (var progress in freshDue)
        {
            if (!wordsById.TryGetValue(progress.WordId, out var word))
                continue;

            cards.Add(new PracticeCard { Word = word, Progress = progress });
            usedWordIds.Add(progress.WordId);
        }
    }

    private async Task AddNewWordsAsync(List<PracticeCard> cards, HashSet<int> usedWordIds, int limit, List<string> topikLevels)
    {
        var newWordIds = await progressRepository.GetNewWordIdsAsync(limit, topikLevels);
        var freshIds = newWordIds.Where(id => !usedWordIds.Contains(id)).ToList();
        if (freshIds.Count == 0)
            return;

        var newWords = await wordRepository.GetByIdsAsync(freshIds);
        var wordsById = newWords.ToDictionary(w => w.Id);

        foreach (var id in freshIds)
        {
            if (!wordsById.TryGetValue(id, out var word))
                continue;

            cards.Add(new PracticeCard { Word = word, Progress = null });
            usedWordIds.Add(id);
        }
    }

    private static bool MatchesTopik(string? wordTopikLevel, List<string> topikLevels) =>
        topikLevels.Count == 0 || topikLevels.Contains((wordTopikLevel ?? string.Empty).Trim());

    private static List<string> NormalizeLevels(List<string>? levels) =>
        (levels ?? new List<string>())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .Distinct()
            .ToList();

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