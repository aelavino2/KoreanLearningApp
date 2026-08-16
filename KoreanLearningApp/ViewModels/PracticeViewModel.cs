using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public partial class PracticeViewModel(
    IWordPracticeService practiceService,
    ISavedWordsService savedWordsService,
    IWordService wordService,
    INavigationService navigationService)
    : ObservableObject
{
    /// <summary>Сколько очков за верный ответ, если он дан очень медленно (минимум).</summary>
    private const int BaseCorrectPoints = 10;

    /// <summary>Через сколько мс после ответа автоматически переходим к следующему вопросу.</summary>
    private const int AnswerRevealDelayMs = 1100;

    private List<PracticeCardViewModel> _sessionCards = new();
    private int _cardIndex;

    /// <summary>
    /// Растёт при каждом старте/сбросе сессии. Нужен, чтобы отложенный переход к
    /// следующему вопросу (после Task.Delay в HandleAnswerAsync) не применился к уже
    /// сброшенной сессии, если пользователь успел нажать "Изменить настройки".
    /// </summary>
    private int _sessionToken;

    /// <summary>Варианты количества карточек для Picker'а на экране настроек.</summary>
    public List<int> WordCountOptions { get; } = new() { 5, 10, 15, 20, 30, 50 };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    /// <summary>false — показан экран настроек, true — идёт сессия квиза.</summary>
    [ObservableProperty]
    private bool _isSessionActive;

    /// <summary>true — все вопросы сессии закончились, показан экран результатов.</summary>
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

    // --- Направление квиза: ровно один из трёх флагов true (радио-группа в XAML) ---

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

    // --- Текущее состояние сессии квиза ---

    [ObservableProperty]
    private PracticeCardViewModel? _currentCard;

    /// <summary>Номер текущего вопроса (1-based) для "Вопрос N из M".</summary>
    [ObservableProperty]
    private int _currentIndex;

    /// <summary>Всего вопросов в сессии.</summary>
    [ObservableProperty]
    private int _totalCount;

    /// <summary>Набранные очки за сессию: +10 плюс бонус за скорость ответа за каждый верный ответ.</summary>
    [ObservableProperty]
    private int _score;

    /// <summary>Сколько ответов из отвеченных оказались верными.</summary>
    [ObservableProperty]
    private int _correctCount;

    [ObservableProperty]
    private string _resultsSummaryText = string.Empty;

    public string QuestionProgressText => TotalCount == 0 ? string.Empty : $"Вопрос {CurrentIndex} из {TotalCount}";

    partial void OnCurrentIndexChanged(int value) => OnPropertyChanged(nameof(QuestionProgressText));

    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(QuestionProgressText));

    /// <summary>
    /// true, когда реально есть текущий вопрос для показа: сессия активна, ещё не все
    /// вопросы отвечены и в сессии вообще есть слова. Используется в XAML вместо трёх
    /// отдельных условий, чтобы экран вопроса не накладывался на "нечего повторять"
    /// или на экран результатов.
    /// </summary>
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

            // Один batch-запрос статуса "сохранено" на всю сессию вместо N вызовов
            // IsSavedAsync по одному на карточку.
            var savedIds = await savedWordsService.GetSavedWordIdSetAsync();

            // Пул слов для отвлекающих вариантов ответа — берём весь словарь, а не только
            // слова этой сессии, чтобы неверные варианты были разнообразнее.
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

    /// <summary>Вернуться к экрану настроек, не завершая приложение и не теряя контекст страницы.</summary>
    [RelayCommand]
    private void BackToSetup()
    {
        StopSession();
        IsSessionActive = false;
        IsSessionComplete = false;
        IsEmpty = false;
    }

    /// <summary>Останавливает таймеры всех карточек сессии — вызывается при уходе со страницы.</summary>
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

    /// <summary>
    /// Строит карточки квиза: для каждого слова сессии определяет направление вопроса,
    /// собирает 1 верный + 3 отвлекающих варианта ответа из общего пула словаря
    /// (с фолбэком на слова этой же сессии, если пул слишком мал) и перемешивает порядок.
    /// </summary>
    private List<PracticeCardViewModel> BuildQuizCards(
        List<PracticeCard> practiceCards,
        HashSet<int> savedIds,
        List<Word> distractorPool,
        QuizDirectionEnum sessionDirection)
    {
        var rng = Random.Shared;
        var result = new List<PracticeCardViewModel>(practiceCards.Count);

        foreach (var card in practiceCards)
        {
            var direction = sessionDirection == QuizDirectionEnum.Random
                ? (rng.Next(2) == 0 ? QuizDirectionEnum.KoreanToTranslation : QuizDirectionEnum.TranslationToKorean)
                : sessionDirection;

            var ruTranslation = card.Word.KrDict?.Senses.FirstOrDefault()?.Ru?.Word;

            // Если у слова нет русского перевода — всегда спрашиваем по корейскому слову,
            // чтобы не показывать пустой вопрос.
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

            // Фолбэк: если в общем пуле не хватило вариантов (маленький словарь) — добираем
            // из других слов этой же сессии.
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

            result.Add(new PracticeCardViewModel(
                card,
                savedWordsService,
                savedIds.Contains(card.Word.Id),
                questionText,
                options,
                OnCardAnswered));
        }

        return result;
    }

    /// <summary>
    /// Колбэк из PracticeCardViewModel, когда пользователь ответил или истекло время.
    /// Начисляет очки, отправляет SM-2 оценку и через паузу переходит к следующему вопросу.
    /// </summary>
    private void OnCardAnswered(PracticeCardViewModel card, bool wasCorrect) => _ = HandleAnswerAsync(card, wasCorrect, _sessionToken);

    private async Task HandleAnswerAsync(PracticeCardViewModel card, bool wasCorrect, int startedWithToken)
    {
        if (wasCorrect)
        {
            CorrectCount++;
            // Бонус за скорость: чем больше секунд осталось на момент ответа, тем больше очков.
            Score += BaseCorrectPoints + card.TimeLeft;
        }

        var rating = !wasCorrect
            ? ReviewRatingEnum.Forgot
            : card.TimeLeft >= PracticeCardViewModel.QuestionSeconds / 2
                ? ReviewRatingEnum.Good
                : ReviewRatingEnum.Hard;

        await practiceService.SubmitReviewAsync(card.WordId, rating);

        await Task.Delay(AnswerRevealDelayMs);

        // Пока ждали, пользователь мог выйти из сессии или начать новую — не трогаем
        // состояние чужой сессии.
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
