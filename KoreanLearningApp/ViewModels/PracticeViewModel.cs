using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public partial class PracticeViewModel(
    IWordPracticeService practiceService, ISavedWordsService savedWordsService,
    IWordService wordService, INavigationService navigationService)
    : ObservableObject
{
    private const int BaseCorrectPoints = 10;
    private const int AnswerRevealDelayMs = 1100;

    private List<PracticeCardViewModel> _sessionCards = new();
    private int _cardIndex;
    private int _sessionToken;

    public List<int> WordCountOptions { get; } = new() { 5, 10, 15, 20, 30, 50 };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isSessionActive;

    [ObservableProperty]
    private bool _isSessionComplete;

    [ObservableProperty]
    private int _wordCount = 20;

    [ObservableProperty]
    private bool _isTopik1Selected;

    [ObservableProperty]
    private bool _isTopik2Selected;

    [ObservableProperty]
    private bool _isTopik3Selected;

    [ObservableProperty]
    private bool _includeSavedWords = true;

    [ObservableProperty]
    private bool _isKoreanToRuDirection = true;

    [ObservableProperty]
    private bool _isRuToKoreanDirection;

    [ObservableProperty]
    private bool _isRandomDirection;

    partial void OnIsKoreanToRuDirectionChanged(bool value)
    {
        if (value)
        {
            IsRuToKoreanDirection = false;
            IsRandomDirection = false;
        }
    }

    partial void OnIsRuToKoreanDirectionChanged(bool value)
    {
        if (value)
        {
            IsKoreanToRuDirection = false;
            IsRandomDirection = false;
        }
    }

    partial void OnIsRandomDirectionChanged(bool value)
    {
        if (value)
        {
            IsKoreanToRuDirection = false;
            IsRuToKoreanDirection = false;
        }
    }

    [ObservableProperty]
    private PracticeCardViewModel? _currentCard;

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _correctCount;

    [ObservableProperty]
    private string _resultsSummaryText = string.Empty;

    public string QuestionProgressText => TotalCount == 0 ? string.Empty : $"Вопрос {CurrentIndex} из {TotalCount}";

    partial void OnCurrentIndexChanged(int value) => OnPropertyChanged(nameof(QuestionProgressText));

    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(QuestionProgressText));

    public bool HasActiveQuestion => IsSessionActive && !IsSessionComplete && !IsEmpty;

    partial void OnIsSessionActiveChanged(bool value) => OnPropertyChanged(nameof(HasActiveQuestion));

    partial void OnIsSessionCompleteChanged(bool value) => OnPropertyChanged(nameof(HasActiveQuestion));

    partial void OnIsEmptyChanged(bool value) => OnPropertyChanged(nameof(HasActiveQuestion));

    [RelayCommand]
    private async Task StartPracticeAsync()
    {
        IsLoading = true;
        try
        {
            var options = new PracticeSessionOptions
            {
                WordCount = WordCount,
                TopikLevels = BuildSelectedTopikLevels(),
                IncludeSavedWords = IncludeSavedWords,
                QuizDirection = BuildSelectedQuizDirection()
            };

            var practiceCards = await practiceService.GetPracticeSessionAsync(options);
            var savedIds = await savedWordsService.GetSavedWordIdSetAsync();
            var distractorPool = await wordService.GetWordsAsync();

            _sessionCards = BuildQuizCards(practiceCards, savedIds, distractorPool, options.QuizDirection);
            _sessionToken++;

            Score = 0;
            CorrectCount = 0;
            TotalCount = _sessionCards.Count;
            _cardIndex = 0;
            IsSessionComplete = false;
            ResultsSummaryText = string.Empty;

            IsEmpty = _sessionCards.Count == 0;
            if (!IsEmpty)
            {
                CurrentCard = _sessionCards[0];
                CurrentIndex = 1;
                CurrentCard.StartTimer();
            }
            else
            {
                CurrentCard = null;
            }

            IsSessionActive = true;
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

    public void StopSession()
    {
        _sessionToken++;

        foreach (var card in _sessionCards)
            card.Dispose();

        CurrentCard = null;
        _sessionCards = new();
        _cardIndex = 0;
    }

    private List<string> BuildSelectedTopikLevels()
    {
        var levels = new List<string>();
        if (IsTopik1Selected) levels.Add("1");
        if (IsTopik2Selected) levels.Add("2");
        if (IsTopik3Selected) levels.Add("3");
        return levels;
    }

    private QuizDirectionEnum BuildSelectedQuizDirection()
    {
        if (IsRuToKoreanDirection) return QuizDirectionEnum.TranslationToKorean;
        if (IsRandomDirection) return QuizDirectionEnum.Random;
        return QuizDirectionEnum.KoreanToTranslation;
    }

    private List<PracticeCardViewModel> BuildQuizCards(
        List<PracticeCard> practiceCards, HashSet<int> savedIds,
        List<Word> distractorPool, QuizDirectionEnum sessionDirection)
    {
        var rng = Random.Shared;
        var result = new List<PracticeCardViewModel>(practiceCards.Count);

        foreach (var card in practiceCards)
        {
            var direction = sessionDirection == QuizDirectionEnum.Random
                ? (rng.Next(2) == 0 ? QuizDirectionEnum.KoreanToTranslation : QuizDirectionEnum.TranslationToKorean)
                : sessionDirection;

            var ruTranslation = card.Word.KrDict?.Senses.FirstOrDefault()?.Ru?.Word;

            var askKoreanShowTranslation = direction == QuizDirectionEnum.KoreanToTranslation
                                            || string.IsNullOrWhiteSpace(ruTranslation);

            var questionText = askKoreanShowTranslation ? card.Word.Korean : ruTranslation!;
            var correctAnswerText = askKoreanShowTranslation
                ? (string.IsNullOrWhiteSpace(ruTranslation) ? "—" : ruTranslation!)
                : card.Word.Korean;

            string? DistractorField(Word w) =>
                askKoreanShowTranslation ? w.KrDict?.Senses.FirstOrDefault()?.Ru?.Word : w.Korean;

            var distractors = distractorPool
                .Where(w => w.Id != card.Word.Id)
                .Select(DistractorField)
                .Where(t => !string.IsNullOrWhiteSpace(t) && !string.Equals(t, correctAnswerText, StringComparison.Ordinal))
                .Distinct()
                .OrderBy(_ => rng.Next())
                .Take(3)
                .Select(t => t!)
                .ToList();

            if (distractors.Count < 3)
            {
                var fallback = practiceCards
                    .Where(c => c.Word.Id != card.Word.Id)
                    .Select(c => askKoreanShowTranslation
                        ? c.Word.KrDict?.Senses.FirstOrDefault()?.Ru?.Word
                        : c.Word.Korean)
                    .Where(t => !string.IsNullOrWhiteSpace(t)
                                && !string.Equals(t, correctAnswerText, StringComparison.Ordinal)
                                && !distractors.Contains(t))
                    .Distinct()
                    .OrderBy(_ => rng.Next())
                    .Select(t => t!);

                distractors.AddRange(fallback.Take(3 - distractors.Count));
            }

            var options = new List<AnswerOptionViewModel> { new(correctAnswerText, isCorrect: true) };
            options.AddRange(distractors.Select(d => new AnswerOptionViewModel(d, isCorrect: false)));
            options = options.OrderBy(_ => rng.Next()).ToList();

            result.Add(new PracticeCardViewModel(card, savedWordsService,
                savedIds.Contains(card.Word.Id), questionText,
                options, OnCardAnswered));
        }

        return result;
    }

    private void OnCardAnswered(PracticeCardViewModel card, bool wasCorrect) => _ = HandleAnswerAsync(card, wasCorrect, _sessionToken);

    private async Task HandleAnswerAsync(PracticeCardViewModel card, bool wasCorrect, int startedWithToken)
    {
        if (wasCorrect)
        {
            CorrectCount++;
            Score += BaseCorrectPoints + card.TimeLeft;
        }

        var rating = !wasCorrect
            ? ReviewRatingEnum.Forgot
            : card.TimeLeft >= PracticeCardViewModel.QuestionSeconds / 2
                ? ReviewRatingEnum.Good
                : ReviewRatingEnum.Hard;

        await practiceService.SubmitReviewAsync(card.WordId, rating);

        await Task.Delay(AnswerRevealDelayMs);

        if (startedWithToken != _sessionToken)
            return;

        AdvanceToNextCard();
    }

    private void AdvanceToNextCard()
    {
        CurrentCard?.Dispose();

        _cardIndex++;
        if (_cardIndex >= _sessionCards.Count)
        {
            CurrentCard = null;
            IsSessionComplete = true;
            ResultsSummaryText = $"Правильных ответов: {CorrectCount} из {TotalCount}";
            return;
        }

        CurrentCard = _sessionCards[_cardIndex];
        CurrentIndex = _cardIndex + 1;
        CurrentCard.StartTimer();
    }

    [RelayCommand]
    private Task GoToWordsAsync() => navigationService.GoToRootAsync(AppRoutes.Words);

    [RelayCommand]
    private Task GoToPracticeAsync() => navigationService.GoToRootAsync(AppRoutes.Practice);

    [RelayCommand]
    private Task GoToImportExportAsync() => navigationService.GoToRootAsync(AppRoutes.ImportExport);

    [RelayCommand]
    private Task GoToSavedAsync() => navigationService.GoToRootAsync(AppRoutes.Saved);
}