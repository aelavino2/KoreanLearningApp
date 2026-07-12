using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;

namespace KoreanLearningApp.Infrastructure.Persistence.Mapping;

internal static class WordMapper
{
    public static Word ToDomain(this WordEntity e) => new()
    {
        Id = e.Id,
        Korean = e.Korean,
        TranscriptionRu = e.TranscriptionRu,
        TranscriptionEn = e.TranscriptionEn,
        TranslationRu = e.TranslationRu,
        TranslationEn = e.TranslationEn,
        RuleExplanation = e.RuleExplanation,
        Category = e.Category,
        Type = e.Type,
        Status = e.Status,
        PronunciationNote = e.PronunciationNote,
        LeitnerBox = e.LeitnerBox,
        NextReviewAt = e.NextReviewAt,
        LastReviewedAt = e.LastReviewedAt
    };

    public static WordEntity ToEntity(this Word w) => new()
    {
        Id = w.Id,
        Korean = w.Korean,
        TranscriptionRu = w.TranscriptionRu,
        TranscriptionEn = w.TranscriptionEn,
        TranslationRu = w.TranslationRu,
        TranslationEn = w.TranslationEn,
        RuleExplanation = w.RuleExplanation,
        Category = w.Category,
        Type = w.Type,
        Status = w.Status,
        PronunciationNote = w.PronunciationNote,
        LeitnerBox = w.LeitnerBox,
        NextReviewAt = w.NextReviewAt,
        LastReviewedAt = w.LastReviewedAt
    };

    public static List<Word> ToDomain(this IEnumerable<WordEntity> entities) => entities.Select(ToDomain).ToList();
}