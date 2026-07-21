using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;

namespace KoreanLearningApp.Infrastructure.Persistence.Mapster;

public static class EntityMappingExtensions
{
    public static WordEntity ToEntity(this Word word)
    {
        return new WordEntity
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
            AudioFile = word.AudioFile
        };
    }

    public static Word ToDomain(this WordEntity entity, List<Sense>? senses = null)
    {
        return new Word
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
            Senses = senses ?? new List<Sense>()
        };
    }

    public static WordSenseEntity ToEntity(this Sense sense, int wordId)
    {
        return new WordSenseEntity
        {
            WordId = wordId,
            DefinitionKo = sense.DefinitionKo,
            EnWord = sense.EnWord,
            EnDefinition = sense.EnDefinition,
            RuWord = sense.RuWord,
            RuDefinition = sense.RuDefinition
        };
    }

    public static Sense ToDomain(this WordSenseEntity entity)
    {
        return new Sense
        {
            DefinitionKo = entity.DefinitionKo,
            EnWord = entity.EnWord,
            EnDefinition = entity.EnDefinition,
            RuWord = entity.RuWord,
            RuDefinition = entity.RuDefinition
        };
    }

    public static KrDictSenseEntity ToEntity(this Sense sense, int krDictId, bool _ = false)
        => new KrDictSenseEntity
        {
            KrDictId = krDictId,
            DefinitionKo = sense.DefinitionKo,
            EnWord = sense.EnWord,
            EnDefinition = sense.EnDefinition,
            RuWord = sense.RuWord,
            RuDefinition = sense.RuDefinition
        };

    public static Sense ToDomain(this KrDictSenseEntity entity)
        => new Sense
        {
            DefinitionKo = entity.DefinitionKo,
            EnWord = entity.EnWord,
            EnDefinition = entity.EnDefinition,
            RuWord = entity.RuWord,
            RuDefinition = entity.RuDefinition
        };

    public static KrDictEntity ToEntity(this KrDict dict)
    {
        return new KrDictEntity
        {
            Id = dict.Id,
            TargetCode = dict.TargetCode,
            Word = dict.Word,
            SupNo = dict.SupNo,
            Pos = dict.Pos,
            Pronunciation = dict.Pronunciation,
            WordGrade = dict.WordGrade,
            Link = dict.Link,
            WordId = dict.WordId,
            AudioId = dict.AudioId
        };
    }

    public static KrDict ToDomain(this KrDictEntity entity, Word parentWord, List<Sense>? senses = null, Audio? audio = null)
    {
        return new KrDict
        {
            Id = entity.Id,
            TargetCode = entity.TargetCode,
            Word = entity.Word,
            SupNo = entity.SupNo,
            Pos = entity.Pos,
            Pronunciation = entity.Pronunciation,
            WordGrade = entity.WordGrade,
            Link = entity.Link,
            WordId = entity.WordId,
            ParentWord = parentWord,
            Senses = senses ?? new List<Sense>(),
            AudioId = entity.AudioId,
            Audio = audio
        };
    }
    public static LangInfoEntity ToEntity(this LangInfo langInfo)
        => new LangInfoEntity { Id = langInfo.Id, Word = langInfo.Word, Definition = langInfo.Definition };

    public static LangInfo ToDomain(this LangInfoEntity entity)
        => new LangInfo { Id = entity.Id, Word = entity.Word, Definition = entity.Definition };
}