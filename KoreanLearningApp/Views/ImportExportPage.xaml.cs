using System.Text.Json;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
namespace KoreanLearningApp.Views;
public partial class ImportExportPage : ContentPage
{
    private readonly DatabaseService _db;
    public ImportExportPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ExportEditor.Text = await _db.ExportWordsAsJsonAsync();
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        var json = ImportEditor.Text;
        if (string.IsNullOrWhiteSpace(json))
        {
            await DisplayAlert("Ошибка", "Вставьте JSON со словами", "ОК");
            return;
        }

        List<WordImportDto>? words;
        try
        {
            words = JsonSerializer.Deserialize<List<WordImportDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            await DisplayAlert("Ошибка формата", $"Не удалось разобрать JSON:\n{ex.Message}", "ОК");
            return;
        }

        if (words is null || words.Count == 0)
        {
            await DisplayAlert("Ошибка", "Список слов пуст", "ОК");
            return;
        }

        var result = await _db.ImportWordsAsync(words);

        ResultLabel.Text = $"Добавлено: {result.AddedCount}. Пропущено: {result.SkippedItems.Count}.";
        if (result.SkippedItems.Count > 0)
            ResultLabel.Text += "\n\n" + string.Join("\n", result.SkippedItems);

        ImportEditor.Text = string.Empty;
        ExportEditor.Text = await _db.ExportWordsAsJsonAsync();
    }

    private async void OnCopyExportClicked(object sender, EventArgs e)
    {
        ExportEditor.Text = await _db.ExportWordsAsJsonAsync();
        await Clipboard.Default.SetTextAsync(ExportEditor.Text);
        await DisplayAlert("Готово", "JSON со всеми текущими словами скопирован в буфер обмена.", "ОК");
    }
}