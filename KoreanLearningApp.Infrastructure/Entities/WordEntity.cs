using SQLite;

namespace KoreanLearningApp.Infrastructure.Entities;

public class WordEntity
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
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PronunciationNote { get; set; } = string.Empty;
    public int LeitnerBox { get; set; } = 1;
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; set; }
}