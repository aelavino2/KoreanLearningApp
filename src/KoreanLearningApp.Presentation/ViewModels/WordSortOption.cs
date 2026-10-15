using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Presentation.ViewModels;

public record WordSortOption(WordSortEnum Sort, string Title)
{
    public static IReadOnlyList<WordSortOption> All { get; } =
    [
        new(WordSortEnum.Rank, "По частоте"),
        new(WordSortEnum.Korean, "По алфавиту"),
        new(WordSortEnum.Tag, "По теме")
    ];
}
