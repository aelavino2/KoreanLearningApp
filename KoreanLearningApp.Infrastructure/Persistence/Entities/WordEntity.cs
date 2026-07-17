using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

public class WordEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Korean { get; set; } = string.Empty;
    public string TranslationRu { get; set; } = string.Empty;
    public string TranslationEn { get; set; } = string.Empty;
    public string RuleExplanation { get; set; } = string.Empty;
    public string PronunciationNote { get; set; } = string.Empty;
}