using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Tests.Support;
using Microsoft.Extensions.Time.Testing;

namespace KoreanLearningApp.Tests.Practice;

public sealed class WordPracticeServiceTests : IAsyncLifetime
{
    private readonly SqliteDatabase _database = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 15, 9, 0, 0, TimeSpan.Zero));
    private readonly TestServices _services;

    public WordPracticeServiceTests()
    {
        _services = new TestServices(_database.Context, _time);
    }

    [Fact]
    public async Task CreateQuizAsync_SavedDueAndNewWords_OrdersSavedThenDueThenNewWithoutDuplicates()
    {
        var saved = await AddWordAsync("저장", rank: 1);
        var due = await AddWordAsync("복습", rank: 2);
        await AddWordAsync("새말", rank: 3);
        await AddWordAsync("다음", rank: 4);
        await _services.PracticeService.SubmitReviewAsync(due, ReviewRatingEnum.Forgot);
        await _services.SavedWordsService.ToggleAsync(saved);
        _time.Advance(TimeSpan.FromDays(2));

        var quiz = await _services.PracticeService.CreateQuizAsync(new PracticeSessionOptions { WordCount = 3 });

        Assert.Equal(["저장", "복습", "새말"], quiz.Select(q => q.Card.Word.Korean));
    }

    [Fact]
    public async Task SubmitReviewAsync_GoodAnswer_WordIsNotOfferedAgainToday()
    {
        var reviewed = await AddWordAsync("사과", rank: 1);
        await AddWordAsync("배", rank: 2);

        await _services.PracticeService.SubmitReviewAsync(reviewed, ReviewRatingEnum.Good);

        var quiz = await _services.PracticeService.CreateQuizAsync(new PracticeSessionOptions { WordCount = 10 });
        Assert.Equal("배", Assert.Single(quiz).Card.Word.Korean);
    }

    [Fact]
    public async Task CreateQuizAsync_TopikLevelFilter_ReturnsOnlyMatchingWords()
    {
        await AddWordAsync("가", rank: 1, topikLevel: "A");
        await AddWordAsync("나", rank: 2, topikLevel: "B");
        await AddWordAsync("다", rank: 3, topikLevel: "C");

        var quiz = await _services.PracticeService.CreateQuizAsync(new PracticeSessionOptions
        {
            WordCount = 10,
            TopikLevels = ["A", "C"]
        });

        Assert.Equal(["가", "다"], quiz.Select(q => q.Card.Word.Korean));
    }

    private Task<int> AddWordAsync(string korean, int rank, string topikLevel = "") =>
        _services.WordRepository.SaveWordAsync(TestWords.Create(korean, ru: $"перевод {korean}", rank: rank, topikLevel: topikLevel));

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _database.DisposeAsync();
}
