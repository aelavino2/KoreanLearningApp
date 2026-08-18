using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Infrastructure.Persistence.Entities;

namespace KoreanLearningApp.Infrastructure.Persistence.Mapster;

internal static class EntityMappingExtensions
{
    public static WordEntity ToEntity(this Word word) => new()
    {
        Id = word.Id,
        Korean = word.Korean,
        RuleExplanation = word.RuleExplanation,
        Rank = word.Rank,
        PartOfSpeech = word.PartOfSpeech,
        Hanja = word.Hanja,
        NiklLevel = word.NiklLevel,
        TopikLevel = word.TopikLevel,
        Status = word.Status,
        SourceIndex = word.SourceIndex,
    };

    public static Word ToDomain(this WordEntity e, KrDict? krDict) => new()
    {
        Id = e.Id,
        Korean = e.Korean,
        RuleExplanation = e.RuleExplanation,
        Rank = e.Rank,
        PartOfSpeech = e.PartOfSpeech,
        Hanja = e.Hanja,
        NiklLevel = e.NiklLevel,
        TopikLevel = e.TopikLevel,
        Status = e.Status,
        SourceIndex = e.SourceIndex,
        KrDict = krDict,
    };

    public static KrDictEntity ToEntity(this KrDict k, int wordId) => new()
    {
        Id = k.Id,
        TargetCode = k.TargetCode,
        Word = k.Word,
        SupNo = k.SupNo,
        Pos = k.Pos,
        Pronunciation = k.Pronunciation,
        WordGrade = k.WordGrade,
        Link = k.Link,
        WordId = wordId,
        AudioId = k.AudioId,
    };

    public static KrDict ToDomain(this KrDictEntity e, List<Sense> senses, Audio? audio) => new()
    {
        Id = e.Id,
        TargetCode = e.TargetCode,
        Word = e.Word,
        SupNo = e.SupNo,
        Pos = e.Pos,
        Pronunciation = e.Pronunciation,
        WordGrade = e.WordGrade,
        Link = e.Link,
        WordId = e.WordId,
        Senses = senses,
        AudioId = e.AudioId,
        Audio = audio,
    };

    public static AudioEntity ToEntity(this Audio a) => new()
    {
        Id = a.Id,
        Url = a.Url,
        File = a.File,
    };

    public static Audio ToDomain(this AudioEntity e) => new()
    {
        Id = e.Id,
        Url = e.Url,
        File = e.File,
    };

    public static LangInfoEntity ToEntity(this LangInfo l) => new()
    {
        Id = l.Id,
        Word = l.Word,
        Definition = l.Definition,
    };

    public static LangInfo ToDomain(this LangInfoEntity e) => new()
    {
        Id = e.Id,
        Word = e.Word,
        Definition = e.Definition,
    };

    public static KrDictSenseEntity ToEntity(this Sense s, int krDictId) => new()
    {
        Id = s.Id,
        KrDictId = krDictId,
        DefinitionKo = s.DefinitionKo,
        EnId = s.EnId,
        RuId = s.RuId,
    };

    public static Sense ToDomain(this KrDictSenseEntity e, LangInfo? en, LangInfo? ru) => new()
    {
        Id = e.Id,
        KrDictId = e.KrDictId,
        DefinitionKo = e.DefinitionKo,
        EnId = e.EnId,
        En = en,
        RuId = e.RuId,
        Ru = ru,
    };

    public static WordProgressEntity ToEntity(this WordProgress p) => new()
    {
        Id = p.Id,
        WordId = p.WordId,
        LearningStatus = (int)p.LearningStatus,
        EasinessFactor = p.EasinessFactor,
        IntervalDays = p.IntervalDays,
        RepetitionCount = p.RepetitionCount,
        NextReviewDate = p.NextReviewDate,
        LastReviewedDate = p.LastReviewedDate,
        TotalReviews = p.TotalReviews,
        TotalCorrect = p.TotalCorrect,
        LapseCount = p.LapseCount,
        FirstSeenAt = p.FirstSeenAt,
    };

    public static WordProgress ToDomain(this WordProgressEntity e) => new()
    {
        Id = e.Id,
        WordId = e.WordId,
        LearningStatus = (LearningStatusEnum)e.LearningStatus,
        EasinessFactor = e.EasinessFactor,
        IntervalDays = e.IntervalDays,
        RepetitionCount = e.RepetitionCount,
        NextReviewDate = e.NextReviewDate,
        LastReviewedDate = e.LastReviewedDate,
        TotalReviews = e.TotalReviews,
        TotalCorrect = e.TotalCorrect,
        LapseCount = e.LapseCount,
        FirstSeenAt = e.FirstSeenAt,
    };

    public static ReviewLogEntity ToEntity(this ReviewLog l) => new()
    {
        Id = l.Id,
        WordId = l.WordId,
        ReviewedAt = l.ReviewedAt,
        Rating = (int)l.Rating,
        IntervalBefore = l.IntervalBefore,
        IntervalAfter = l.IntervalAfter,
        EasinessBefore = l.EasinessBefore,
        EasinessAfter = l.EasinessAfter,
    };

    public static ReviewLog ToDomain(this ReviewLogEntity e) => new()
    {
        Id = e.Id,
        WordId = e.WordId,
        ReviewedAt = e.ReviewedAt,
        Rating = (ReviewRatingEnum)e.Rating,
        IntervalBefore = e.IntervalBefore,
        IntervalAfter = e.IntervalAfter,
        EasinessBefore = e.EasinessBefore,
        EasinessAfter = e.EasinessAfter,
    };

    public static SavedWordEntity ToEntity(this SavedWord s) => new()
    {
        Id = s.Id,
        WordId = s.WordId,
        SavedAt = s.SavedAt,
    };

    public static SavedWord ToDomain(this SavedWordEntity e) => new()
    {
        Id = e.Id,
        WordId = e.WordId,
        SavedAt = e.SavedAt,
    };
}