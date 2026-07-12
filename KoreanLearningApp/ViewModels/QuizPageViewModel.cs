using KoreanLearningApp.Models;
using KoreanLearningApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using KoreanLearningApp.Services;
using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.ViewModels
{
    public class QuizPageViewModel : INotifyPropertyChanged
    {
        private const int QuestionSeconds = 20;
        private const int MinWordsForQuiz = 4;

        private readonly DatabaseService _db;
        private readonly QuizSessionSettings _settings;
        private readonly Random _random = new();

        private List<Word> _allWords = new();
        private List<Word> _quizQueue = new();
        private int _currentIndex;
        private int _correctCount;
        private double _timeLeft;
        private IDispatcherTimer? _timer;
        private bool _answerLocked;
        private Word? _currentWord;

        private int? _pendingCho;
        private int? _pendingJung;
        private int? _pendingJong;
        private readonly StringBuilder _committedAnswer = new();

        public ObservableCollection<QuizOptionViewModel> Options { get; } = new()
    {
        new QuizOptionViewModel(),
        new QuizOptionViewModel(),
        new QuizOptionViewModel(),
        new QuizOptionViewModel()
    };

        public ObservableCollection<string> ChoKeys { get; } = new(HangulComposer.Cho);
        public ObservableCollection<string> JungKeys { get; } = new(HangulComposer.Jung);
        public ObservableCollection<string> JongKeys { get; } = new(HangulComposer.CommonJong);

        public ICommand OptionCommand { get; }
        public ICommand ChoKeyCommand { get; }
        public ICommand JungKeyCommand { get; }
        public ICommand JongKeyCommand { get; }
        public ICommand CommitSyllableCommand { get; }
        public ICommand SpaceCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand SubmitAnswerCommand { get; }
        public ICommand FinishCommand { get; }

        public QuizPageViewModel(DatabaseService db, QuizSessionSettings settings)
        {
            _db = db;
            _settings = settings;

            OptionCommand = new Command<QuizOptionViewModel>(async option => await OnOptionSelectedAsync(option));
            ChoKeyCommand = new Command<string>(OnChoKey);
            JungKeyCommand = new Command<string>(OnJungKey);
            JongKeyCommand = new Command<string>(OnJongKey);
            CommitSyllableCommand = new Command(() =>
            {
                CommitPendingSyllable();
                UpdateTypedAnswerDisplay();
            });
            SpaceCommand = new Command(() =>
            {
                CommitPendingSyllable();
                _committedAnswer.Append(' ');
                UpdateTypedAnswerDisplay();
            });
            BackspaceCommand = new Command(OnBackspace);
            SubmitAnswerCommand = new Command(async () => await OnSubmitAnswerAsync());
            FinishCommand = new Command(async () => await OnFinishAsync());
        }

        public bool IsNormalMode => _settings.Difficulty == QuizDifficulty.Normal;
        public bool IsHardMode => _settings.Difficulty == QuizDifficulty.Hard;

        private string _progressText = string.Empty;
        public string ProgressText
        {
            get => _progressText;
            set { _progressText = value; OnPropertyChanged(); }
        }

        private string _scoreText = "Правильно: 0";
        public string ScoreText
        {
            get => _scoreText;
            set { _scoreText = value; OnPropertyChanged(); }
        }

        private string _boxText = string.Empty;
        public string BoxText
        {
            get => _boxText;
            set { _boxText = value; OnPropertyChanged(); }
        }

        private string _questionText = string.Empty;
        public string QuestionText
        {
            get => _questionText;
            set { _questionText = value; OnPropertyChanged(); }
        }

        private double _timerProgress = 1;
        public double TimerProgress
        {
            get => _timerProgress;
            set { _timerProgress = value; OnPropertyChanged(); }
        }

        private bool _isTimeRunningOut;
        public bool IsTimeRunningOut
        {
            get => _isTimeRunningOut;
            set { _isTimeRunningOut = value; OnPropertyChanged(); }
        }

        private string _typedAnswerText = " ";
        public string TypedAnswerText
        {
            get => _typedAnswerText;
            set { _typedAnswerText = value; OnPropertyChanged(); }
        }

        private bool _isTypedCorrect;
        public bool IsTypedCorrect
        {
            get => _isTypedCorrect;
            set { _isTypedCorrect = value; OnPropertyChanged(); }
        }

        private bool _isTypedWrong;
        public bool IsTypedWrong
        {
            get => _isTypedWrong;
            set { _isTypedWrong = value; OnPropertyChanged(); }
        }

        public async Task StartQuizAsync()
        {
            _allWords = await _db.GetWordsAsync();

            if (_allWords.Count < MinWordsForQuiz)
            {
                await CustomAlertPage.ShowAsync(
                    "Недостаточно слов",
                    $"Для проверки знаний нужно минимум {MinWordsForQuiz} слов в словаре. Сейчас их: {_allWords.Count}.",
                    "Понятно");
                await Shell.Current.GoToAsync("..");
                return;
            }

            var sessionSize = Math.Clamp(_settings.WordCount, 1, 30);
            var due = await _db.GetDueWordsAsync(sessionSize);
            _quizQueue = due.OrderBy(_ => _random.Next()).Take(sessionSize).ToList();

            if (_quizQueue.Count == 0)
            {
                await CustomAlertPage.ShowAsync(
                    "Пока нечего повторять",
                    "Все слова уже повторены недавно. Возвращайтесь позже, либо добавьте новые слова.",
                    "Понятно");
                await Shell.Current.GoToAsync("..");
                return;
            }

            _currentIndex = 0;
            _correctCount = 0;
            ScoreText = "Правильно: 0";

            ShowQuestion();
        }

        private void ShowQuestion()
        {
            if (_currentIndex >= _quizQueue.Count)
            {
                _ = FinishQuizAsync();
                return;
            }

            _answerLocked = false;
            _currentWord = _quizQueue[_currentIndex];
            ProgressText = $"Вопрос {_currentIndex + 1} из {_quizQueue.Count}";
            BoxText = SpacedRepetitionHelper.ProgressStars(_currentWord.LeitnerBox);

            if (_settings.Difficulty == QuizDifficulty.Normal)
            {
                QuestionText = _currentWord.Korean;
                SetupNormalOptions();
            }
            else
            {
                QuestionText = _currentWord.TranslationRu;
                ResetHangulInput();
            }

            StartTimer();
        }

        private void SetupNormalOptions()
        {
            if (_currentWord is null) return;

            var wrongOptions = _allWords
                .Where(w => w.Id != _currentWord.Id && !string.IsNullOrWhiteSpace(w.TranslationRu))
                .Select(w => w.TranslationRu)
                .Distinct()
                .OrderBy(_ => _random.Next())
                .Take(3)
                .ToList();

            var options = wrongOptions.Append(_currentWord.TranslationRu).OrderBy(_ => _random.Next()).ToList();

            for (int i = 0; i < Options.Count; i++)
            {
                var option = Options[i];
                option.Reset();
                option.Text = i < options.Count ? options[i] : string.Empty;
                option.IsVisible = i < options.Count;
            }
        }

        // --- Клавиатура хангыля ---

        private void ResetHangulInput()
        {
            _pendingCho = null;
            _pendingJung = null;
            _pendingJong = null;
            _committedAnswer.Clear();
            IsTypedCorrect = false;
            IsTypedWrong = false;
            UpdateTypedAnswerDisplay();
        }

        private void UpdateTypedAnswerDisplay()
        {
            var preview = BuildPendingPreview();
            var text = _committedAnswer.ToString() + preview;
            TypedAnswerText = string.IsNullOrEmpty(text) ? " " : text;
        }

        private string BuildPendingPreview()
        {
            if (_pendingCho is null)
                return string.Empty;

            if (_pendingJung is null)
                return HangulComposer.Cho[_pendingCho.Value];

            var composed = HangulComposer.Compose(_pendingCho.Value, _pendingJung.Value, _pendingJong ?? 0);
            return composed?.ToString() ?? string.Empty;
        }

        private void CommitPendingSyllable()
        {
            if (_pendingCho is not null && _pendingJung is not null)
            {
                var composed = HangulComposer.Compose(_pendingCho.Value, _pendingJung.Value, _pendingJong ?? 0);
                if (composed is not null)
                    _committedAnswer.Append(composed.Value);
            }
            _pendingCho = null;
            _pendingJung = null;
            _pendingJong = null;
        }

        private void OnChoKey(string? key)
        {
            if (key is null) return;
            var idx = Array.IndexOf(HangulComposer.Cho, key);
            if (idx < 0) return;

            // Если уже был полный слог (cho+jung) — фиксируем его и начинаем новый
            if (_pendingCho is not null && _pendingJung is not null)
                CommitPendingSyllable();

            _pendingCho = idx;
            _pendingJung = null;
            _pendingJong = null;
            UpdateTypedAnswerDisplay();
        }

        private void OnJungKey(string? key)
        {
            if (key is null || _pendingCho is null) return;
            var idx = Array.IndexOf(HangulComposer.Jung, key);
            if (idx < 0) return;

            _pendingJung = idx;
            _pendingJong = null;
            UpdateTypedAnswerDisplay();
        }

        private void OnJongKey(string? key)
        {
            if (key is null || _pendingCho is null || _pendingJung is null) return;
            var idx = Array.IndexOf(HangulComposer.Jong, key);
            if (idx < 0) return;

            _pendingJong = idx;
            CommitPendingSyllable();
            UpdateTypedAnswerDisplay();
        }

        private void OnBackspace()
        {
            if (_pendingJong is not null) _pendingJong = null;
            else if (_pendingJung is not null) _pendingJung = null;
            else if (_pendingCho is not null) _pendingCho = null;
            else if (_committedAnswer.Length > 0) _committedAnswer.Remove(_committedAnswer.Length - 1, 1);

            UpdateTypedAnswerDisplay();
        }

        private async Task OnSubmitAnswerAsync()
        {
            if (_answerLocked || _currentWord is null) return;

            CommitPendingSyllable();
            _answerLocked = true;
            StopTimer();

            var typed = _committedAnswer.ToString().Trim();
            bool isCorrect = string.Equals(typed, _currentWord.Korean.Trim(), StringComparison.Ordinal);

            await RecordAnswerAsync(isCorrect);

            if (isCorrect)
            {
                _correctCount++;
                ScoreText = $"Правильно: {_correctCount}";
                IsTypedCorrect = true;
            }
            else
            {
                IsTypedWrong = true;
                TypedAnswerText = $"{typed}  ?  {_currentWord.Korean}";
            }

            await Task.Delay(1200);
            IsTypedCorrect = false;
            IsTypedWrong = false;
            MoveToNextQuestion();
        }

        // --- Обычный режим (выбор варианта) ---

        private async Task OnOptionSelectedAsync(QuizOptionViewModel? option)
        {
            if (_answerLocked || option is null || _currentWord is null)
                return;

            _answerLocked = true;
            StopTimer();

            bool isCorrect = option.Text == _currentWord.TranslationRu;
            await RecordAnswerAsync(isCorrect);

            if (isCorrect)
            {
                _correctCount++;
                ScoreText = $"Правильно: {_correctCount}";
                option.IsCorrectAnswer = true;
            }
            else
            {
                option.IsWrongAnswer = true;
                HighlightCorrectAnswer();
            }

            await Task.Delay(700);
            MoveToNextQuestion();
        }

        private void HighlightCorrectAnswer()
        {
            if (_currentWord is null) return;

            foreach (var option in Options)
            {
                if (option.Text == _currentWord.TranslationRu)
                    option.IsCorrectAnswer = true;
            }
        }

        // --- Общая логика таймера / прогресса ---

        private void StartTimer()
        {
            StopTimer();
            _timeLeft = QuestionSeconds;
            TimerProgress = 1;
            IsTimeRunningOut = false;

            _timer = Application.Current!.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        public void StopTimer()
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
            TimerProgress = fraction;
            IsTimeRunningOut = fraction < 0.3;

            if (_timeLeft <= 0 && !_answerLocked)
            {
                _answerLocked = true;
                StopTimer();
                await RecordAnswerAsync(wasCorrect: false);

                if (_settings.Difficulty == QuizDifficulty.Normal)
                    HighlightCorrectAnswer();
                else if (_currentWord is not null)
                    TypedAnswerText = $"Время вышло: {_currentWord.Korean}";

                await Task.Delay(900);
                MoveToNextQuestion();
            }
        }

        private async Task RecordAnswerAsync(bool wasCorrect)
        {
            if (_currentWord is null) return;
            SpacedRepetitionHelper.ApplyAnswer(_currentWord, wasCorrect);
            await _db.SaveWordAsync(_currentWord);
        }

        private void MoveToNextQuestion()
        {
            _currentIndex++;
            ShowQuestion();
        }

        private async Task FinishQuizAsync()
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

        private async Task OnFinishAsync()
        {
            bool confirm = await CustomAlertPage.ShowConfirmAsync(
                "Завершить проверку?",
                "Прогресс по уже отвеченным словам сохранён. Оставшиеся слова останутся в очереди на следующий раз.",
                "Завершить",
                "Продолжить");

            if (confirm)
            {
                StopTimer();
                await Shell.Current.GoToAsync("..");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
