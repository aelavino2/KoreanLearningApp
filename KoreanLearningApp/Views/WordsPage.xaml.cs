using System.Collections.ObjectModel;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;
namespace KoreanLearningApp.Views;

public partial class WordsPage : ContentPage
{
    private readonly DatabaseService _db;
    private bool _koreanToRussian = true;
    private readonly Random _random = new();
    private List<Word> _allWords = new();
    private LearningStatus? _statusFilter = null;

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
        var filtered = _statusFilter is null
            ? _allWords
            : _allWords.Where(w => w.Status == _statusFilter).ToList();

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

    private void OnFilterAllClicked(object sender, EventArgs e)
    {
        _statusFilter = null;
        ApplyFilter();
    }

    private void OnFilterLearningClicked(object sender, EventArgs e)
    {
        _statusFilter = LearningStatus.Learning;
        ApplyFilter();
    }

    private void OnFilterLearnedClicked(object sender, EventArgs e)
    {
        _statusFilter = LearningStatus.Learned;
        ApplyFilter();
    }

    private void OnToggleDirectionClicked(object sender, EventArgs e)
    {
        _koreanToRussian = !_koreanToRussian;
        DirectionButton.Text = _koreanToRussian ? "🇰🇷 → 🇷🇺" : "🇷🇺 → 🇰🇷";
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
        {
            await DisplayAlert("Правило", card.RuleText, "Понятно");
        }
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