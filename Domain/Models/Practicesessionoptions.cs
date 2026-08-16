namespace KoreanLearningApp.Domain.Models;

/// <summary>
/// Настройки сессии практики, которые пользователь задаёт на экране запуска
/// перед тем, как появятся карточки.
/// </summary>
public class PracticeSessionOptions
{
    /// <summary>Сколько карточек показать за сессию.</summary>
    public int WordCount { get; set; } = 20;

    /// <summary>
    /// Уровни TOPIK для фильтрации ("1", "2", "3"). Пустой список — без фильтра,
    /// берутся слова всех уровней.
    /// </summary>
    public List<string> TopikLevels { get; set; } = new();

    /// <summary>
    /// Включать ли сохранённые (избранные) слова в сессию. Если true — они идут
    /// первыми в очереди, приоритетнее due-слов и новых слов.
    /// </summary>
    public bool IncludeSavedWords { get; set; } = true;
}