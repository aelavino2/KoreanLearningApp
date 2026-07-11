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
    public string PronunciationNote { get; set; } = string.Empty;

    // Алгоритм интервального повторения (Лейтнер): коробка 1-5, чем выше — тем реже повторяется
    public int LeitnerBox { get; set; } = 1;

    // Когда слово в следующий раз должно попасть в квиз. По умолчанию — "уже сейчас",
    // чтобы новые слова сразу участвовали в первой сессии повторения.
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;

    // Когда слово последний раз проверялось в квизе (для статистики)
    public DateTime? LastReviewedAt { get; set; }
}