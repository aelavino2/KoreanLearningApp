namespace KoreanLearningApp.Domain.Models.Enums;

/// <summary>
/// Направление вопроса в квизе с выбором варианта: что показываем как вопрос,
/// а что — как 4 варианта ответа.
/// </summary>
public enum QuizDirectionEnum
{
    /// <summary>Показываем корейское слово — выбираем русский перевод.</summary>
    KoreanToTranslation = 0,

    /// <summary>Показываем русский перевод — выбираем корейское слово.</summary>
    TranslationToKorean = 1,

    /// <summary>Направление выбирается случайно для каждой карточки.</summary>
    Random = 2
}
