using KoreanLearningApp.Models;
using KoreanLearningApp.Services;

namespace KoreanLearningApp.Views;

public partial class AddWordPage : ContentPage
{
    private readonly DatabaseService _db;

    public AddWordPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KoreanEntry.Text))
        {
            await DisplayAlert("Ошибка", "Введите слово на корейском", "ОК");
            return;
        }

        var word = new Word
        {
            Korean = KoreanEntry.Text,
            TranscriptionRu = TranscriptionRuEntry.Text ?? string.Empty,
            TranscriptionEn = TranscriptionEnEntry.Text ?? string.Empty,
            TranslationRu = TranslationRuEntry.Text ?? string.Empty,
            TranslationEn = TranslationEnEntry.Text ?? string.Empty
        };

        await _db.SaveWordAsync(word);
        await Shell.Current.GoToAsync("..");
    }
}