using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IQuizQuestionFactory
{
    /// <summary>
    /// Строит вопросы с одним правильным и до трёх неправильных вариантов.
    /// Неправильные варианты берутся из <paramref name="distractorPool"/>, при нехватке — из других карточек сессии.
    /// </summary>
    List<QuizQuestion> Create(IReadOnlyList<PracticeCard> cards, QuizDirectionEnum direction, IReadOnlyList<Word> distractorPool);
}
