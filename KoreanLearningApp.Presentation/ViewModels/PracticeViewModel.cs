using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Presentation.Dialogs;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Presentation.ViewModels;

public partial class PracticeViewModel(
    IWordPracticeService practiceService,
    ISavedWordsService savedWordsService,
    IQuizScoringService scoringService,
    IDialogService dialogService,
    TimeProvider timeProvider) : ObservableObject
{
    private static readonly TimeSpan AnswerRevealDelay = TimeSpan.FromMilliseconds(1100);

    private List<PracticeCardViewModel> _sessionCards = [];
    private int _cardIndex;
    private int _sessionVersion;

    public IReadOnlyList<int> WordCountOptions { get; } = [5, 10, 15, 20, 30, 50];

    public IReadOnlyList<TopikLevelOptionViewModel> TopikLevelOptions { get; } = Enum.GetValues<TopikLevelEnum>()
        .Where(level => level != TopikLevelEnum.Empty)
        .Select(level => new TopikLevelOptionViewModel(level.ToCode()))
        .ToList();

    [ObservableProperty]
    private int _wordCount = 20;

    [ObservableProperty]
    private bool _includeSavedWords = true;

    [ObservableProperty]
    private QuizDirectionEnum _quizDirection = QuizDirectionEnum.KoreanToTranslation;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartPracticeCommand))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveQuestion))]
    private bool _isSessionActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveQuestion))]
    private bool _isSessionComplete;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveQuestion))]
    private bool _isEmpty;

    [ObservableProperty]
    private PracticeCardViewModel? _currentCard;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(QuestionProgressText))]
    private int _currentIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(QuestionProgressText), nameof(ResultsSummaryText))]
    private int _totalCount;

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResultsSummaryText))]
    private int _correctCount;

    public bool HasActiveQuestion => IsSessionActive && !IsSessionComplete && !IsEmpty;

    public string QuestionProgressText => TotalCount == 0 ? string.Empty : $"Вопрос {CurrentIndex} из {TotalCount}";

    public string ResultsSummaryText => $"Правильных ответов: {CorrectCount} из {TotalCount}";

    public void StopSession()
    {
        _sessionVersion++;

        foreach (var card in _sessionCards)
            card.Dispose();

        _sessionCards = [];
        _cardIndex = 0;
        CurrentCard = null;
    }

    private bool CanStartPractice() => !IsLoading;

    [RelayCommand(CanExecute = nameof(CanStartPractice))]
    private async Task StartPracticeAsync()
    {
        IsLoading = true;
        try
        {
            StopSession();

            var questions = await practiceService.CreateQuizAsync(BuildSessionOptions());
            var savedIds = await savedWordsService.GetSavedWordIdsAsync();

            _sessionCards = questions
                .Select(q => new PracticeCardViewModel(q, savedIds.Contains(q.Card.Word.Id),
                    savedWordsService, dialogService, timeProvider, OnCardAnswered))
                .ToList();

            Score = 0;
            CorrectCount = 0;
            TotalCount = _sessionCards.Count;
            IsSessionComplete = false;
            IsEmpty = _sessionCards.Count == 0;
            IsSessionActive = true;

            if (!IsEmpty)
                ShowCard(0);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось начать квиз: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BackToSetup()
    {
        StopSession();
        IsSessionActive = false;
        IsSessionComplete = false;
        IsEmpty = false;
    }

    private PracticeSessionOptions BuildSessionOptions() => new()
    {
        WordCount = WordCount,
        IncludeSavedWords = IncludeSavedWords,
        QuizDirection = QuizDirection,
        TopikLevels = TopikLevelOptions.Where(o => o.IsSelected).Select(o => o.Code).ToList()
    };

    private void OnCardAnswered(PracticeCardViewModel card, bool wasCorrect) =>
        _ = HandleAnswerAsync(card, wasCorrect, _sessionVersion);

    private async Task HandleAnswerAsync(PracticeCardViewModel card, bool wasCorrect, int sessionVersion)
    {
        if (wasCorrect)
            CorrectCount++;

        Score += scoringService.CalculateScore(wasCorrect, card.TimeLeft);

        try
        {
            var rating = scoringService.DetermineRating(wasCorrect, card.TimeLeft, PracticeCardViewModel.QuestionSeconds);
            await practiceService.SubmitReviewAsync(card.WordId, rating);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось сохранить результат: {ex.Message}");
        }

        await Task.Delay(AnswerRevealDelay, timeProvider);

        if (sessionVersion == _sessionVersion)
            ShowCard(_cardIndex + 1);
    }

    private void ShowCard(int index)
    {
        CurrentCard?.Dispose();

        if (index >= _sessionCards.Count)
        {
            CurrentCard = null;
            IsSessionComplete = true;
            return;
        }

        _cardIndex = index;
        CurrentIndex = index + 1;
        CurrentCard = _sessionCards[index];
        CurrentCard.StartTimer();
    }
}
