using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services.Practice;

public sealed class QuizQuestionFactory(Random random) : IQuizQuestionFactory
{
    public const string NoTranslationPlaceholder = "—";

    private const int DistractorCount = 3;

    public List<QuizQuestion> Create(IReadOnlyList<PracticeCard> cards, QuizDirectionEnum direction,
        IReadOnlyList<Word> distractorPool)
    {
        var sessionWords = cards.Select(c => c.Word).ToList();
        return cards
            .Select(card => CreateQuestion(card, ResolveDirection(direction), distractorPool, sessionWords))
            .ToList();
    }

    private QuizQuestion CreateQuestion(PracticeCard card, QuizDirectionEnum direction,
        IReadOnlyList<Word> distractorPool, IReadOnlyList<Word> sessionWords)
    {
        var translation = GetTranslation(card.Word);
        var askKorean = direction == QuizDirectionEnum.KoreanToTranslation || translation is null;

        Func<Word, string?> answerOf = askKorean ? GetTranslation : w => w.Korean;
        var correctAnswer = askKorean ? translation ?? NoTranslationPlaceholder : card.Word.Korean;

        var options = PickDistractors(card.Word.Id, correctAnswer, answerOf, distractorPool, sessionWords)
            .Select(text => new QuizAnswerOption { Text = text, IsCorrect = false })
            .Append(new QuizAnswerOption { Text = correctAnswer, IsCorrect = true });

        return new QuizQuestion
        {
            Card = card,
            QuestionText = askKorean ? card.Word.Korean : translation!,
            Options = Shuffle(options).ToList()
        };
    }

    private IEnumerable<string> PickDistractors(int wordId, string correctAnswer, Func<Word, string?> answerOf,
        IReadOnlyList<Word> distractorPool, IReadOnlyList<Word> sessionWords) =>
        Shuffle(distractorPool)
            .Concat(Shuffle(sessionWords))
            .Where(w => w.Id != wordId)
            .Select(answerOf)
            .OfType<string>()
            .Where(text => !string.IsNullOrWhiteSpace(text) && !string.Equals(text, correctAnswer, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Take(DistractorCount);

    private QuizDirectionEnum ResolveDirection(QuizDirectionEnum direction)
    {
        if (direction != QuizDirectionEnum.Random)
            return direction;

        return random.Next(2) == 0 ? QuizDirectionEnum.KoreanToTranslation : QuizDirectionEnum.TranslationToKorean;
    }

    private IEnumerable<T> Shuffle<T>(IEnumerable<T> items) => items.OrderBy(_ => random.Next());

    private static string? GetTranslation(Word word)
    {
        var translation = word.KrDict?.Senses.FirstOrDefault()?.Ru?.Word;
        return string.IsNullOrWhiteSpace(translation) ? null : translation;
    }
}
