using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Services.SpacedRepetition;

/// <summary>
/// Результат одного применения формулы SM-2.
/// </summary>
/// <param name="EasinessFactor">Новый Easiness Factor (не опускается ниже 1.3).</param>
/// <param name="IntervalDays">Новый интервал до следующего повторения, в днях.</param>
/// <param name="RepetitionCount">Новый счётчик подряд идущих успешных повторений (0, если случился провал).</param>
/// <param name="IsLapse">true — пользователь не вспомнил слово (Forgot), это "провал" повторения.</param>
public readonly record struct Sm2Result(
    double EasinessFactor,
    int IntervalDays,
    int RepetitionCount,
    bool IsLapse);

/// <summary>
/// Чистая реализация формулы SM-2 (Wozniak, 1987), адаптированная под 3-уровневую оценку ответа
/// вместо оригинальной шкалы 0-5. Не имеет побочных эффектов и не знает про хранение данных —
/// только считает новые EF/интервал/счётчик повторений на основе текущего состояния и оценки.
/// </summary>
public static class Sm2Formula
{
    private const double MinEasinessFactor = 1.3;
    private const double StartingEasinessFactor = 2.5;

    /// <summary>
    /// Соответствие нашей 3-уровневой шкалы (Forgot/Hard/Good) оригинальной шкале качества q (0-5) из SM-2.
    /// q &lt; 3 в оригинале означает провал — поэтому Forgot сознательно ниже этого порога.
    /// </summary>
    private static int MapRatingToQuality(ReviewRatingEnum rating) => rating switch
    {
        ReviewRatingEnum.Forgot => 2,
        ReviewRatingEnum.Hard => 3,
        ReviewRatingEnum.Good => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(rating), rating, "Неизвестная оценка ответа")
    };

    /// <summary>
    /// Стартовые значения для слова, которое ещё ни разу не повторялось.
    /// </summary>
    public static Sm2Result Initial() => new(
        EasinessFactor: StartingEasinessFactor,
        IntervalDays: 0,
        RepetitionCount: 0,
        IsLapse: false);

    /// <summary>
    /// Считает новое состояние слова по формуле SM-2 на основе текущего состояния и оценки ответа.
    /// </summary>
    public static Sm2Result Calculate(
        double currentEasinessFactor,
        int currentIntervalDays,
        int currentRepetitionCount,
        ReviewRatingEnum rating)
    {
        var q = MapRatingToQuality(rating);

        // Шаг 1: обновляем Easiness Factor по стандартной формуле SM-2.
        // При q=5 (Good) EF растёт сильнее всего, при q=2 (Forgot) — падает.
        var newEasinessFactor = currentEasinessFactor + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
        if (newEasinessFactor < MinEasinessFactor)
            newEasinessFactor = MinEasinessFactor;

        // Шаг 2: провал (q < 3) — интервал и счётчик повторений сбрасываются.
        // EF при этом не обнуляется полностью — штраф уже учтён в шаге 1, история слова не теряется целиком.
        if (q < 3)
        {
            return new Sm2Result(
                EasinessFactor: newEasinessFactor,
                IntervalDays: 1,
                RepetitionCount: 0,
                IsLapse: true);
        }

        // Шаг 3: успешное повторение — считаем новый интервал по классической схеме SM-2:
        // I(1) = 1, I(2) = 6, I(n) = I(n-1) * EF для n > 2.
        var newRepetitionCount = currentRepetitionCount + 1;
        var newIntervalDays = newRepetitionCount switch
        {
            1 => 1,
            2 => 6,
            _ => (int)Math.Round(currentIntervalDays * newEasinessFactor, MidpointRounding.AwayFromZero)
        };

        return new Sm2Result(
            EasinessFactor: newEasinessFactor,
            IntervalDays: newIntervalDays,
            RepetitionCount: newRepetitionCount,
            IsLapse: false);
    }
}