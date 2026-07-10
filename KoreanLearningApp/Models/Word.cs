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

    // Пояснение: почему слово так пишется или произносится
    // (звуковые изменения, грамматическое правило и т.п.)
    public string RuleExplanation { get; set; } = string.Empty;
}