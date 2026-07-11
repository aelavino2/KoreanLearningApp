using SQLite;

namespace KoreanLearningApp.Models;

public class Word
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Korean { get; set; } = string.Empty;
    public string TranscriptionRu { get; set; } = string.Empty;
    public string TranscriptionEn { get; set; } = string.Empty;
    public string TranslationRu { get; set; } = string.Empty;
    public string TranslationEn { get; set; } = string.Empty;
    public string RuleExplanation { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public WordType Type { get; set; } = WordType.Other;

    public LearningStatus Status { get; set; } = LearningStatus.Learning;
}