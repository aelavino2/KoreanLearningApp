using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Domain.Models;

public class PracticeSessionOptions
{
    public int WordCount { get; set; } = 20;

    public List<string> TopikLevels { get; set; } = new();

    public bool IncludeSavedWords { get; set; } = true;

    public QuizDirectionEnum QuizDirection { get; set; } = QuizDirectionEnum.KoreanToTranslation;
}