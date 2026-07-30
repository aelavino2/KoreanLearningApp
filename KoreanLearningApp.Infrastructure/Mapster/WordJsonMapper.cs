using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Import.DTO;

namespace KoreanLearningApp.Infrastructure.Import;

internal static class WordImportMapper
{
    public static Word ToDomain(this WordJsonDto dto)
    {
        var word = new Word
        {
            Korean = !string.IsNullOrWhiteSpace(dto.KrDict?.Word) ? dto.KrDict!.Word : dto.Word,
            RuleExplanation = dto.Explanation,
            Rank = ParseRank(dto.Rank),
            PartOfSpeech = dto.PartOfSpeech,
            Hanja = dto.Hanja,
            NiklLevel = dto.NiklLevel,
            TopikLevel = dto.TopikLevel,
            Status = dto.Status,
            SourceIndex = dto.Idx,
        };

        word.KrDict = dto.KrDict?.ToDomain();

        return word;
    }

    private static KrDict ToDomain(this KrDictJsonDto dto) => new()
    {
        TargetCode = dto.TargetCode,
        Word = dto.Word,
        SupNo = dto.SupNo,
        Pos = dto.Pos,
        Pronunciation = dto.Pronunciation,
        WordGrade = dto.WordGrade,
        Link = dto.Link,
        Audio = dto.Audio?.ToDomain(),
        Senses = dto.Senses.Select(s => s.ToDomain()).ToList(),
    };

    private static Audio ToDomain(this AudioJsonDto dto) => new()
    {
        Url = dto.Url,
        File = dto.File,
    };

    private static Sense ToDomain(this SenseJsonDto dto) => new()
    {
        DefinitionKo = dto.DefinitionKo,
        En = dto.En?.ToDomain(),
        Ru = dto.Ru?.ToDomain(),
    };

    private static LangInfo ToDomain(this LangInfoJsonDto dto) => new()
    {
        Word = dto.Word,
        Definition = dto.Definition,
    };

    private static int ParseRank(string rank) => int.TryParse(rank, out var value) ? value : 0;
}