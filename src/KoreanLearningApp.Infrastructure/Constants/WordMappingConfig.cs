using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Import.DTO;
using Mapster;

namespace KoreanLearningApp.Infrastructure.Constants;

public static class WordMappingConfig
{
    private static bool _configured;
    private static readonly object Lock = new();

    public static void Configure()
    {
        if (_configured) return;

        lock (Lock)
        {
            if (_configured) return;

            TypeAdapterConfig<WordJsonDto, Word>.NewConfig()
                .Map(dest => dest.Korean, src => ResolveKorean(src))
                .Map(dest => dest.Rank, src => ParseRank(src.Rank))
                .Map(dest => dest.RuleExplanation, src => src.Explanation)
                .Map(dest => dest.SourceIndex, src => src.Idx);

            TypeAdapterConfig<Word, WordJsonDto>.NewConfig()
                .Map(dest => dest.Word, src => src.Korean)
                .Map(dest => dest.Explanation, src => src.RuleExplanation)
                .Map(dest => dest.Idx, src => src.SourceIndex)
                .Map(dest => dest.Rank, src => src.Rank.ToString());

            _configured = true;
        }
    }

    private static string ResolveKorean(WordJsonDto src) =>
        !string.IsNullOrWhiteSpace(src.KrDict?.Word) ? src.KrDict!.Word : src.Word;

    private static int ParseRank(string rank) => int.TryParse(rank, out var value) ? value : 0;
}