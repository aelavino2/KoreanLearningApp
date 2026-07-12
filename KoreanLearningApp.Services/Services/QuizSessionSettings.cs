using KoreanLearningApp.Models;
namespace KoreanLearningApp.Services;

// Простой держатель выбранных настроек сессии — заполняется на странице настроек,
// считывается страницей квиза. Регистрируется как Singleton в DI.
public class QuizSessionSettings
{
    public QuizDifficulty Difficulty { get; set; } = QuizDifficulty.Normal;
    public int WordCount { get; set; } = 10;
}