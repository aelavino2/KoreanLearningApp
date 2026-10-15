using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Presentation.Dialogs;
using KoreanLearningApp.Presentation.ViewModels;
using KoreanLearningApp.Services.Abstractions.Services;
using KoreanLearningApp.Services.Services.Practice;
using KoreanLearningApp.Tests.Support;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KoreanLearningApp.Tests.ViewModels;

public class PracticeViewModelTests
{
    private const int WordId = 1;

    private readonly IWordPracticeService _practiceService = Substitute.For<IWordPracticeService>();
    private readonly ISavedWordsService _savedWordsService = Substitute.For<ISavedWordsService>();
    private readonly FakeTimeProvider _time = new();
    private readonly TaskCompletionSource<ReviewRatingEnum> _submittedRating = new();
    private readonly PracticeViewModel _viewModel;

    public PracticeViewModelTests()
    {
        _savedWordsService.GetSavedWordIdsAsync().Returns(new HashSet<int>());
        _practiceService.SubmitReviewAsync(WordId, Arg.Any<ReviewRatingEnum>())
            .Returns(call =>
            {
                _submittedRating.TrySetResult(call.Arg<ReviewRatingEnum>());
                return Task.CompletedTask;
            });

        _viewModel = new PracticeViewModel(_practiceService, _savedWordsService, new QuizScoringService(),
            Substitute.For<IDialogService>(), _time);
    }

    [Fact]
    public async Task StartPracticeCommand_NoWordsToPractice_ShowsEmptyState()
    {
        _practiceService.CreateQuizAsync(Arg.Any<PracticeSessionOptions>()).Returns(new List<QuizQuestion>());

        await _viewModel.StartPracticeCommand.ExecuteAsync(null);

        Assert.True(_viewModel.IsEmpty);
        Assert.False(_viewModel.HasActiveQuestion);
    }

    [Fact]
    public async Task SelectAnswer_CorrectAnswerImmediately_ScoresAndSubmitsGoodRating()
    {
        await StartQuizWithOneQuestionAsync();
        var card = _viewModel.CurrentCard!;

        card.SelectAnswerCommand.Execute(card.Options.Single(o => o.IsCorrect));

        Assert.Equal(ReviewRatingEnum.Good, await _submittedRating.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(1, _viewModel.CorrectCount);
        Assert.Equal(20, _viewModel.Score);
    }

    [Fact]
    public async Task Timer_TimeRunsOut_RevealsCorrectOptionAndSubmitsForgot()
    {
        await StartQuizWithOneQuestionAsync();
        var card = _viewModel.CurrentCard!;

        _time.Advance(TimeSpan.FromSeconds(PracticeCardViewModel.QuestionSeconds + 1));

        Assert.Equal(ReviewRatingEnum.Forgot, await _submittedRating.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.True(card.Options.Single(o => o.IsCorrect).IsRevealedCorrect);
        Assert.Equal(0, _viewModel.CorrectCount);
    }

    [Fact]
    public async Task SelectAnswer_LastQuestion_CompletesSessionAfterRevealDelay()
    {
        await StartQuizWithOneQuestionAsync();
        var card = _viewModel.CurrentCard!;
        card.SelectAnswerCommand.Execute(card.Options.Single(o => !o.IsCorrect));
        await _submittedRating.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _time.Advance(TimeSpan.FromSeconds(2));

        await _viewModel.WaitUntilAsync(() => _viewModel.IsSessionComplete);
        Assert.Equal("Правильных ответов: 0 из 1", _viewModel.ResultsSummaryText);
    }

    private async Task StartQuizWithOneQuestionAsync()
    {
        var question = new QuizQuestion
        {
            Card = new PracticeCard { Word = TestWords.Create("사과", ru: "яблоко", id: WordId) },
            QuestionText = "사과",
            Options =
            [
                new QuizAnswerOption { Text = "яблоко", IsCorrect = true },
                new QuizAnswerOption { Text = "груша", IsCorrect = false }
            ]
        };
        _practiceService.CreateQuizAsync(Arg.Any<PracticeSessionOptions>()).Returns(new List<QuizQuestion> { question });

        await _viewModel.StartPracticeCommand.ExecuteAsync(null);
    }
}
