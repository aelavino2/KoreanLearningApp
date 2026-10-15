using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Tests.Support;

public static class TestWords
{
    public static Word Create(string korean, string? ru = null, string? en = null, int rank = 1,
        string topikLevel = "", int supNo = 0, int id = 0) => new()
    {
        Id = id,
        Korean = korean,
        Rank = rank,
        TopikLevel = topikLevel,
        PartOfSpeech = PartOfSpeechEnum.Noun,
        KrDict = new KrDict
        {
            Word = korean,
            SupNo = supNo,
            Pos = PartOfSpeechEnum.Noun,
            Senses = ru is null && en is null
                ? []
                : [CreateSense(ru, en)]
        }
    };

    public static Word WithTags(this Word word, params Tag[] tags)
    {
        word.Tags = [.. tags];
        return word;
    }

    public static Tag Tag(string nameKo, string nameRu) => new() { NameKo = nameKo, NameRu = nameRu };

    public static Tag WithId(this Tag tag, int id)
    {
        tag.Id = id;
        return tag;
    }

    public static Sense CreateSense(string? ru, string? en) => new()
    {
        Ru = ru is null ? null : new LangInfo { Word = ru },
        En = en is null ? null : new LangInfo { Word = en }
    };
}
