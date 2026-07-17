using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;

namespace KoreanLearningApp.Infrastructure.Persistence.Mapster;

internal static class WordMapper
{
    public static Word ToDomain(this WordEntity entity) => new()
    {
        Id = entity.Id,
        Korean = entity.Korean,
        TranslationRu = entity.TranslationRu,
        TranslationEn = entity.TranslationEn,
        RuleExplanation = entity.RuleExplanation,
        PronunciationNote = entity.PronunciationNote,
    };

    public static WordEntity ToEntity(this Word word) => new()
    {
        Id = word.Id,
        Korean = word.Korean,
        TranslationRu = word.TranslationRu,
        TranslationEn = word.TranslationEn,
        RuleExplanation = word.RuleExplanation,
        PronunciationNote = word.PronunciationNote,
    };
}