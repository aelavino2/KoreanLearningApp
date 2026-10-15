using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Services.Practice;
using KoreanLearningApp.Tests.Support;

namespace KoreanLearningApp.Tests.Practice;

public class QuizQuestionFactoryTests
{
    private readonly QuizQuestionFactory _factory = new(new Random(7));

    [Fact]
    public void Create_EnoughDistractors_OneCorrectAndThreeDistinctWrongOptions()
    {
        var card = new PracticeCard { Word = TestWords.Create("사과", ru: "яблоко", id: 1) };
        var pool = new[] { "груша", "банан", "банан", "яблоко", "слива", "вишня" }
            .Select((ru, i) => TestWords.Create($"단어{i}", ru: ru, id: i + 10))
            .ToList();

        var question = Assert.Single(_factory.Create([card], QuizDirectionEnum.KoreanToTranslation, pool));

        Assert.Equal("사과", question.QuestionText);
        Assert.Equal("яблоко", Assert.Single(question.Options, o => o.IsCorrect).Text);
        Assert.Equal(4, question.Options.Count);
        Assert.Equal(4, question.Options.Select(o => o.Text).Distinct().Count());
    }

    [Fact]
    public void Create_TranslationToKoreanWithoutTranslation_FallsBackToKoreanQuestion()
    {
        var card = new PracticeCard { Word = TestWords.Create("사과", id: 1) };

        var question = Assert.Single(_factory.Create([card], QuizDirectionEnum.TranslationToKorean, []));

        Assert.Equal("사과", question.QuestionText);
        Assert.Equal(QuizQuestionFactory.NoTranslationPlaceholder, Assert.Single(question.Options).Text);
    }
}
