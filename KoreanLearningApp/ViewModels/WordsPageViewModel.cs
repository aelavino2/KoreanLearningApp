using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Events;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
using KoreanLearningApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace KoreanLearningApp.ViewModels;


public class WordsPageViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _db;
    private readonly Random _random = new();

    private List<Word> _allWords = new();
    private LearningStatus? _statusFilter;
    private bool _koreanToRussian = true;
    private Locale? _koreanLocale;
    private bool _localeChecked;

    public ObservableCollection<WordCardViewModel> Cards { get; } = new();

    public event EventHandler<AlertRequestEventArgs>? AlertRequested;

    public event EventHandler<string>? NavigationRequested;

    public ICommand FilterAllCommand { get; }
    public ICommand FilterLearningCommand { get; }
    public ICommand FilterLearnedCommand { get; }
    public ICommand ToggleDirectionCommand { get; }
    public ICommand ShuffleCommand { get; }
    public ICommand RuleCommand { get; }
    public ICommand StatusToggleCommand { get; }
    public ICommand PlayAudioCommand { get; }
    public ICommand RevealCommand { get; }
    public ICommand QuizCommand { get; }
    public ICommand ImportExportCommand { get; }
    public ICommand AddCommand { get; }

    public WordsPageViewModel(DatabaseService db)
    {
        _db = db;

        FilterAllCommand = new Command(() => SetFilter(null));
        FilterLearningCommand = new Command(() => SetFilter(LearningStatus.Learning));
        FilterLearnedCommand = new Command(() => SetFilter(LearningStatus.Learned));
        ToggleDirectionCommand = new Command(ToggleDirection);
        ShuffleCommand = new Command(Shuffle);
        RuleCommand = new Command<WordCardViewModel>(ShowRule);
        StatusToggleCommand = new Command<WordCardViewModel>(async card => await ToggleStatusAsync(card));
        PlayAudioCommand = new Command<WordCardViewModel>(async card => await PlayAudioAsync(card));
        RevealCommand = new Command<WordCardViewModel>(card =>
        {
            if (card is not null)
                card.IsRevealed = true;
        });
        QuizCommand = new Command(() => NavigationRequested?.Invoke(this, nameof(QuizSettingsPage)));
        ImportExportCommand = new Command(() => NavigationRequested?.Invoke(this, nameof(ImportExportPage)));
        AddCommand = new Command(() => NavigationRequested?.Invoke(this, nameof(AddWordPage)));
    }

    public string DirectionButtonText => _koreanToRussian ? "KR - RU" : "RU - KR";

    public bool IsFilterAllActive => _statusFilter is null;
    public bool IsFilterLearningActive => _statusFilter == LearningStatus.Learning;
    public bool IsFilterLearnedActive => _statusFilter == LearningStatus.Learned;

    public async Task LoadWordsAsync()
    {
        _allWords = await _db.GetWordsAsync();
        ApplyFilter();
    }

    private void SetFilter(LearningStatus? status)
    {
        _statusFilter = status;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _statusFilter is null
            ? _allWords
            : _allWords.Where(w => w.Status == _statusFilter).ToList();

        Cards.Clear();
        foreach (var word in filtered)
            Cards.Add(new WordCardViewModel(word, _koreanToRussian));

        RenumberCards();

        OnPropertyChanged(nameof(IsFilterAllActive));
        OnPropertyChanged(nameof(IsFilterLearningActive));
        OnPropertyChanged(nameof(IsFilterLearnedActive));
    }

    private void RenumberCards()
    {
        for (int i = 0; i < Cards.Count; i++)
            Cards[i].Number = i + 1;
    }

    private void ToggleDirection()
    {
        _koreanToRussian = !_koreanToRussian;
        OnPropertyChanged(nameof(DirectionButtonText));
        foreach (var card in Cards)
            card.SetDirection(_koreanToRussian);
    }

    private void Shuffle()
    {
        var shuffled = Cards.OrderBy(_ => _random.Next()).ToList();
        Cards.Clear();
        foreach (var card in shuffled)
            Cards.Add(card);
        RenumberCards();
    }

    private void ShowRule(WordCardViewModel? card)
    {
        if (card is null)
            return;

        AlertRequested?.Invoke(this, new AlertRequestEventArgs("Правило", card.RuleText, "Понятно"));
    }

    private async Task ToggleStatusAsync(WordCardViewModel? card)
    {
        if (card is null)
            return;

        card.ToggleStatus();
        await _db.SaveWordAsync(card.UnderlyingWord);

        if (_statusFilter is not null && card.UnderlyingWord.Status != _statusFilter)
        {
            Cards.Remove(card);
            RenumberCards();
        }
    }

    private async Task<Locale?> GetKoreanLocaleAsync()
    {
        if (_localeChecked)
            return _koreanLocale;

        var locales = await TextToSpeech.Default.GetLocalesAsync();
        _koreanLocale = locales.FirstOrDefault(l =>
            l.Language.StartsWith("ko", StringComparison.OrdinalIgnoreCase));
        _localeChecked = true;
        return _koreanLocale;
    }

    private async Task PlayAudioAsync(WordCardViewModel? card)
    {
        if (card is null)
            return;

        var locale = await GetKoreanLocaleAsync();
        if (locale is null)
        {
            AlertRequested?.Invoke(this, new AlertRequestEventArgs(
                "Корейский голос не найден",
                "На этом устройстве не установлен голосовой синтез для корейского языка. " +
                "На Android: Настройки -> Язык и ввод -> Синтез речи (TTS) -> установить корейский языковой пакет Google. " +
                "Без этого системная озвучка корейских слов недоступна.",
                "ОК"));
            return;
        }

        try
        {
            await TextToSpeech.Default.SpeakAsync(card.UnderlyingWord.Korean, new SpeechOptions
            {
                Locale = locale,
                Pitch = 1.0f,
                Volume = 1.0f
            });
        }
        catch (Exception ex)
        {
            AlertRequested?.Invoke(this, new AlertRequestEventArgs("Ошибка воспроизведения", ex.Message, "ОК"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}