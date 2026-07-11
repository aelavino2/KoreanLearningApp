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
        TypePicker.ItemsSource = WordTypeHelper.AllTypes.Select(t => t.Label).ToList();
        TypePicker.SelectedIndex = WordTypeHelper.AllTypes.ToList().FindIndex(t => t.Type == WordType.Other);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KoreanEntry.Text))
        {
            await DisplayAlert("Ошибка", "Введите слово на корейском", "ОК");
            return;
        }

        var selectedType = TypePicker.SelectedItem is string label
            ? WordTypeHelper.FromLabel(label)
            : WordType.Other;

        var word = new Word
        {
            Korean = KoreanEntry.Text,
            TranscriptionRu = TranscriptionRuEntry.Text ?? string.Empty,
            TranscriptionEn = TranscriptionEnEntry.Text ?? string.Empty,
            TranslationRu = TranslationRuEntry.Text ?? string.Empty,
            TranslationEn = TranslationEnEntry.Text ?? string.Empty,
            RuleExplanation = RuleExplanationEditor.Text ?? string.Empty,
            Category = CategoryEntry.Text ?? string.Empty,
            Type = selectedType,
            Status = LearningStatus.Learning,
            PronunciationNote = PronunciationNoteEditor.Text ?? string.Empty
        };
        await _db.SaveWordAsync(word);
        await Shell.Current.GoToAsync("..");
    }
}