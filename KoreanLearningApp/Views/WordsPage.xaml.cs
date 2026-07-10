using System.Collections.ObjectModel;
using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class WordsPage : ContentPage
{
    private readonly DatabaseService _db;
    private bool _koreanToRussian = true;

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
    }

    private void OnToggleDirectionClicked(object sender, EventArgs e)
    {
        _koreanToRussian = !_koreanToRussian;
        DirectionButton.Text = _koreanToRussian ? "???? ? ????" : "???? ? ????";
        foreach (var card in Cards)
            card.SetDirection(_koreanToRussian);
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