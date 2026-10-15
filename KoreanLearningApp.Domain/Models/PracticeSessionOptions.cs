using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Domain.Models;

public class PracticeSessionOptions
{
    public int WordCount { get; init; } = 20;

    /// <summary>
    /// Коды уровней TOPIK (как в <see cref="Word.TopikLevel"/>). Пустой список — все уровни.
    /// </summary>
    public IReadOnlyCollection<string> TopikLevels { get; init; } = [];

    public bool IncludeSavedWords { get; init; } = true;

    public QuizDirectionEnum QuizDirection { get; init; } = QuizDirectionEnum.KoreanToTranslation;
}
