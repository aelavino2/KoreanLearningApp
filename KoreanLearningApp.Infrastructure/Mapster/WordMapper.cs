using System.Text.Json;
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

        Rank = entity.Rank,
        PartOfSpeech = entity.PartOfSpeech,
        Hanja = entity.Hanja,
        NiklLevel = entity.NiklLevel,
        TopikLevel = entity.TopikLevel,
        Status = entity.Status,
        SourceIndex = entity.SourceIndex,

        TargetCode = entity.TargetCode,
        SupNo = entity.SupNo,
        Pos = entity.Pos,
        WordGrade = entity.WordGrade,
        DictLink = entity.DictLink,

        AudioUrl = entity.AudioUrl,
        AudioFile = entity.AudioFile,

        Senses = JsonSerializer.Deserialize<List<Sense>>(entity.SensesJson) ?? new List<Sense>(),
    };

    public static WordEntity ToEntity(this Word word) => new()
    {
        Id = word.Id,
        Korean = word.Korean,
        TranslationRu = word.TranslationRu,
        TranslationEn = word.TranslationEn,
        RuleExplanation = word.RuleExplanation,
        PronunciationNote = word.PronunciationNote,

        Rank = word.Rank,
        PartOfSpeech = word.PartOfSpeech,
        Hanja = word.Hanja,
        NiklLevel = word.NiklLevel,
        TopikLevel = word.TopikLevel,
        Status = word.Status,
        SourceIndex = word.SourceIndex,

        TargetCode = word.TargetCode,
        SupNo = word.SupNo,
        Pos = word.Pos,
        WordGrade = word.WordGrade,
        DictLink = word.DictLink,

        AudioUrl = word.AudioUrl,
        AudioFile = word.AudioFile,

        SensesJson = JsonSerializer.Serialize(word.Senses),
        UniqueKey = $"{word.Korean}_{word.SupNo}",
    };
}