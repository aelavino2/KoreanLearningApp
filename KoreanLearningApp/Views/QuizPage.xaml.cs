using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
namespace KoreanLearningApp.Views;

public partial class QuizPage : ContentPage
{
    private const int QuestionSeconds = 15;
    private const int MinLearnedWords = 4;

    private readonly DatabaseService _db;
    private readonly Random _random = new();

    private List<Word> _allWords = new();
    private List<Word> _quizQueue = new();
    private int _currentIndex;
    private int _correctCount;
    private double _timeLeft;
    private IDispatcherTimer? _timer;
    private bool _answerLocked;
    private Word? _currentWord;

    public QuizPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await StartQuizAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTimer();
    }

    private async Task StartQuizAsync()
    {
        _allWords = await _db.GetWordsAsync();
        var learned = _allWords.Where(w => w.Status == LearningStatus.Learned).ToList();

        if (learned.Count < MinLearnedWords)
        {
            await CustomAlertPage.ShowAsync(
                "Недостаточно выученных слов",
                $"Для проверки знаний нужно минимум {MinLearnedWords} слов со статусом \"Выучено\". " +
                $"Сейчас их: {learned.Count}. Отметьте больше слов как выученные и возвращайтесь.",
                "Понятно");
            await Shell.Current.GoToAsync("..");
            return;
        }

        _quizQueue = learned.OrderBy(_ => _random.Next()).ToList();
        _currentIndex = 0;
        _correctCount = 0;
        ScoreLabel.Text = "Правильно: 0";

        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (_currentIndex >= _quizQueue.Count)
        {
            FinishQuiz();
            return;
        }

        _answerLocked = false;
        _currentWord = _quizQueue[_currentIndex];
        ProgressLabel.Text = $"Вопрос {_currentIndex + 1} из {_quizQueue.Count}";
        QuestionLabel.Text = _currentWord.Korean;

        var wrongOptions = _allWords
            .Where(w => w.Id != _currentWord.Id && !string.IsNullOrWhiteSpace(w.TranslationRu))
            .Select(w => w.TranslationRu)
            .Distinct()
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToList();

        var options = wrongOptions.Append(_currentWord.TranslationRu).OrderBy(_ => _random.Next()).ToList();

        var buttons = new[] { Option1Button, Option2Button, Option3Button, Option4Button };
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Text = i < options.Count ? options[i] : string.Empty;
            buttons[i].IsVisible = i < options.Count;
            buttons[i].BackgroundColor = (Color)this.Resources["CardColor"];
            buttons[i].TextColor = (Color)this.Resources["InkColor"];
        }

        StartTimer();
    }

    private void StartTimer()
    {
        StopTimer();
        _timeLeft = QuestionSeconds;
        TimerBar.Progress = 1;
        TimerBar.ProgressColor = (Color)this.Resources["JadeColor"];

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        _timeLeft -= 0.1;
        var fraction = Math.Max(_timeLeft / QuestionSeconds, 0);
        TimerBar.Progress = fraction;

        if (fraction < 0.3)
            TimerBar.ProgressColor = (Color)this.Resources["PersimmonColor"];

        if (_timeLeft <= 0 && !_answerLocked)
        {
            _answerLocked = true;
            StopTimer();
            HighlightCorrectAnswer();
            await Task.Delay(900);
            MoveToNextQuestion();
        }
    }

    private async void OnOptionClicked(object? sender, EventArgs e)
    {
        if (_answerLocked || sender is not Button clicked || _currentWord is null)
            return;

        _answerLocked = true;
        StopTimer();

        bool isCorrect = clicked.Text == _currentWord.TranslationRu;
        if (isCorrect)
        {
            _correctCount++;
            ScoreLabel.Text = $"Правильно: {_correctCount}";
            clicked.BackgroundColor = (Color)this.Resources["JadeColor"];
            clicked.TextColor = Colors.White;
        }
        else
        {
            clicked.BackgroundColor = (Color)this.Resources["PersimmonColor"];
            clicked.TextColor = Colors.White;
            HighlightCorrectAnswer();
        }

        await Task.Delay(700);
        MoveToNextQuestion();
    }

    private void HighlightCorrectAnswer()
    {
        if (_currentWord is null)
            return;

        var buttons = new[] { Option1Button, Option2Button, Option3Button, Option4Button };
        foreach (var button in buttons)
        {
            if (button.Text == _currentWord.TranslationRu)
            {
                button.BackgroundColor = (Color)this.Resources["JadeColor"];
                button.TextColor = Colors.White;
            }
        }
    }

    private void MoveToNextQuestion()
    {
        _currentIndex++;
        ShowQuestion();
    }

    private async void FinishQuiz()
    {
        StopTimer();
        var total = _quizQueue.Count;
        var percent = total == 0 ? 0 : (int)Math.Round(_correctCount * 100.0 / total);

        await CustomAlertPage.ShowAsync(
            "Результат",
            $"Правильных ответов: {_correctCount} из {total} ({percent}%)",
            "Готово");

        await Shell.Current.GoToAsync("..");
    }

    private async void OnFinishClicked(object sender, EventArgs e)
    {
        bool confirm = await CustomAlertPage.ShowConfirmAsync(
            "Завершить проверку?",
            "Текущий результат не будет сохранён, если выйти сейчас.",
            "Завершить",
            "Продолжить");

        if (confirm)
        {
            StopTimer();
            await Shell.Current.GoToAsync("..");
        }
    }
}