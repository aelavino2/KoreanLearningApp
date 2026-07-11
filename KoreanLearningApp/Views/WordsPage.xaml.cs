using System.Collections.ObjectModel;
using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;
namespace KoreanLearningApp.Views;
public partial class WordsPage : ContentPage
{
    private readonly DatabaseService _db;
    private bool _koreanToRussian = true;
    private readonly Random _random = new();
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
        var words = await _db.GetWordsAsync();
        Cards.Clear();
        foreach (var word in words)
            Cards.Add(new WordCardViewModel(word, _koreanToRussian));
        RenumberCards();
    }
    private void RenumberCards()
    {
        for (int i = 0; i < Cards.Count; i++)
            Cards[i].Number = i + 1;
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
        // Алгоритм Фишера-Йетса: перемешиваем список карточек на месте
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