// Infrastructure/Import/WordImportMapper.cs
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Import.DTO;

namespace KoreanLearningApp.Infrastructure.Import;

internal static class WordImportMapper
{
    public static Word ToDomain(this WordJsonDto dto)
    {
        var krdict = dto.KrDict;
        var firstSense = krdict?.Senses.FirstOrDefault();

        return new Word
        {
            Korean = dto.Word,
            TranslationEn = firstSense?.En?.Word ?? string.Empty,
            TranslationRu = firstSense?.Ru?.Word ?? string.Empty,
            RuleExplanation = dto.Explanation,
            PronunciationNote = krdict?.Pronunciation ?? string.Empty,

            Rank = ParseRank(dto.Rank),
            PartOfSpeech = dto.PartOfSpeech,
            Hanja = dto.Hanja,
            NiklLevel = dto.NiklLevel,
            TopikLevel = dto.TopikLevel,
            Status = dto.Status,
            SourceIndex = dto.Idx,

            TargetCode = krdict?.TargetCode ?? string.Empty,
            SupNo = krdict?.SupNo ?? 0,
            Pos = krdict?.Pos ?? string.Empty,
            WordGrade = krdict?.WordGrade ?? string.Empty,
            DictLink = krdict?.Link ?? string.Empty,

            AudioUrl = krdict?.Audio?.Url ?? string.Empty,
            AudioFile = krdict?.Audio?.File ?? string.Empty,

            Senses = MapSenses(krdict?.Senses),
        };
    }

    private static List<Sense> MapSenses(List<SenseJsonDto>? senses)
    {
        if (senses is null || senses.Count == 0)
            return new List<Sense>();

        return senses
            .Select(s => new Sense
            {
                DefinitionKo = s.DefinitionKo,
                EnWord = s.En?.Word ?? string.Empty,
                EnDefinition = s.En?.Definition ?? string.Empty,
                RuWord = s.Ru?.Word ?? string.Empty,
                RuDefinition = s.Ru?.Definition ?? string.Empty,
            })
            .ToList();
    }

    private static int ParseRank(string rank) => int.TryParse(rank, out var value) ? value : 0;
}