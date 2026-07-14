using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;

namespace KoreanLearningApp.Infrastructure.Persistence.Mapster;

internal static class WordMapper
{
    public static Word ToDomain(this WordEntity entity) => new()
    {
        Id = entity.Id,
        Korean = entity.Korean,
        TranscriptionRu = entity.TranscriptionRu,
        TranscriptionEn = entity.TranscriptionEn,
        TranslationRu = entity.TranslationRu,
        TranslationEn = entity.TranslationEn,
        RuleExplanation = entity.RuleExplanation,
        Category = entity.Category,
        PronunciationNote = entity.PronunciationNote,
        LeitnerBox = entity.LeitnerBox,
        NextReviewAt = entity.NextReviewAt,
        LastReviewedAt = entity.LastReviewedAt
    };

    public static WordEntity ToEntity(this Word word) => new()
    {
        Id = word.Id,
        Korean = word.Korean,
        TranscriptionRu = word.TranscriptionRu,
        TranscriptionEn = word.TranscriptionEn,
        TranslationRu = word.TranslationRu,
        TranslationEn = word.TranslationEn,
        RuleExplanation = word.RuleExplanation,
        Category = word.Category,
        PronunciationNote = word.PronunciationNote,
        LeitnerBox = word.LeitnerBox,
        NextReviewAt = word.NextReviewAt,
        LastReviewedAt = word.LastReviewedAt
    };
}