namespace KoreanLearningApp.Domain.Models;

/// <summary>
/// Слово, подготовленное для показа в практике: словарные данные + текущий прогресс.
/// Progress == null означает, что слово ещё ни разу не показывалось (это "новое" слово).
/// </summary>
public class PracticeCard
{
    public Word Word { get; set; } = null!;
    public WordProgress? Progress { get; set; }

    public bool IsNew => Progress is null;
}