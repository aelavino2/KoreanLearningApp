using SQLite;

namespace KoreanLearningApp.Models;

public class Word
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Слово на корейском (хангыль), напр. "안녕하세요"
    public string Korean { get; set; } = string.Empty;

    // Транскрипция кириллицей, напр. "аннёнхасэё"
    public string TranscriptionRu { get; set; } = string.Empty;

    // Романизация латиницей, напр. "annyeonghaseyo"
    public string TranscriptionEn { get; set; } = string.Empty;

    // Перевод на русский
    public string TranslationRu { get; set; } = string.Empty;

    // Перевод на английский
    public string TranslationEn { get; set; } = string.Empty;
}