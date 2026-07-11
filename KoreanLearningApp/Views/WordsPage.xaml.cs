using System.Collections.ObjectModel;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;
using Microsoft.Maui.Media;
namespace KoreanLearningApp.Views;

public partial class WordsPage : ContentPage
{
    private readonly DatabaseService _db;
    private bool _koreanToRussian = true;
    private readonly Random _random = new();
    private List<Word> _allWords = new();
    private LearningStatus? _statusFilter = null;
    private Locale? _koreanLocale;
    private bool _localeChecked;

    public ObservableCollection<WordCardViewModel> Cards { get; } = new();

    public WordsPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWordsAsync();
    }

    private async Task LoadWordsAsync()
    {
        _allWords = await _db.GetWordsAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _statusFilter is null ? _allWords : _allWords.Where(w => w.Status == _statusFilter).ToList();
        Cards.Clear();
        foreach (var word in filtered)
            Cards.Add(new WordCardViewModel(word, _koreanToRussian));
        RenumberCards();
        UpdateFilterButtonStyles();
    }

    private void UpdateFilterButtonStyles()
    {
        var active = (Color)this.Resources["JadeColor"];
        var inactive = (Color)this.Resources["CardColor"];

        FilterAllButton.BackgroundColor = _statusFilter is null ? active : inactive;
        FilterAllButton.TextColor = _statusFilter is null ? Colors.White : active;
        FilterLearningButton.BackgroundColor = _statusFilter == LearningStatus.Learning ? active : inactive;
        FilterLearningButton.TextColor = _statusFilter == LearningStatus.Learning ? Colors.White : active;
        FilterLearnedButton.BackgroundColor = _statusFilter == LearningStatus.Learned ? active : inactive;
        FilterLearnedButton.TextColor = _statusFilter == LearningStatus.Learned ? Colors.White : active;
    }

    private void RenumberCards()
    {
        for (int i = 0; i < Cards.Count; i++)
            Cards[i].Number = i + 1;
    }

    private void OnFilterAllClicked(object sender, EventArgs e) { _statusFilter = null; ApplyFilter(); }
    private void OnFilterLearningClicked(object sender, EventArgs e) { _statusFilter = LearningStatus.Learning; ApplyFilter(); }
    private void OnFilterLearnedClicked(object sender, EventArgs e) { _statusFilter = LearningStatus.Learned; ApplyFilter(); }

    private void OnToggleDirectionClicked(object sender, EventArgs e)
    {
        _koreanToRussian = !_koreanToRussian;
        DirectionButton.Text = _koreanToRussian ? "KR - RU" : "RU - KR";
        foreach (var card in Cards)
            card.SetDirection(_koreanToRussian);
    }

    private void OnShuffleClicked(object sender, EventArgs e)
    {
        var shuffled = Cards.OrderBy(_ => _random.Next()).ToList();
        Cards.Clear();
        foreach (var card in shuffled)
            Cards.Add(card);
        RenumberCards();
    }

    private async void OnRuleClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: WordCardViewModel card })
            await DisplayAlert("Правило", card.RuleText, "Понятно");
    }

    private async void OnStatusToggleClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: WordCardViewModel card })
        {
            card.ToggleStatus();
            await _db.SaveWordAsync(card.UnderlyingWord);
            if (_statusFilter is not null && card.UnderlyingWord.Status != _statusFilter)
            {
                Cards.Remove(card);
                RenumberCards();
            }
        }
    }

    // Ищем корейский голос среди установленных на устройстве TTS-голосов.
    // Если пользователь не установил корейский языковой пакет в системе — вернёт null.
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

    private async void OnQuizClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(QuizPage));
    }

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: WordCardViewModel card })
            return;

        var locale = await GetKoreanLocaleAsync();
        if (locale is null)
        {
            await DisplayAlert(
                "Корейский голос не найден",
                "На этом устройстве не установлен голосовой синтез для корейского языка. " +
                "На Android: Настройки -> Язык и ввод -> Синтез речи (TTS) -> установить корейский языковой пакет Google. " +
                "Без этого системная озвучка корейских слов недоступна.",
                "ОК");
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
            await DisplayAlert("Ошибка воспроизведения", ex.Message, "ОК");
        }
    }

    private async void OnImportExportClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ImportExportPage));
    }

    private void OnRevealClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: WordCardViewModel card })
            card.IsRevealed = true;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddWordPage));
    }
}