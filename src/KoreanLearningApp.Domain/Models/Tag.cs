namespace KoreanLearningApp.Domain.Models;

/// <summary>
/// Тематическая категория слова. <see cref="NameKo"/> — уникальный ключ тега.
/// </summary>
public class Tag
{
    public int Id { get; set; }
    public string NameKo { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
}
