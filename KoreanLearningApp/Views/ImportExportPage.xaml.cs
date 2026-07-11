using System.Text.Json;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
namespace KoreanLearningApp.Views;

public partial class ImportExportPage : ContentPage
{
    private readonly DatabaseService _db;

    private static readonly FilePickerFileType JsonFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".json" } },
        { DevicePlatform.Android, new[] { "application/json" } },
        { DevicePlatform.iOS, new[] { "public.json" } },
        { DevicePlatform.MacCatalyst, new[] { "json" } },
    });

    public ImportExportPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BackupPathLabel.Text = $"Резервная копия автоматически сохраняется здесь при каждом изменении словаря:\n{DatabaseService.GetBackupFilePath()}";
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите JSON файл со словами",
                FileTypes = JsonFileType
            });

            if (result is null)
                return; // пользователь отменил выбор

            var json = await File.ReadAllTextAsync(result.FullPath);

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
                await CustomAlertPage.ShowAsync("Ошибка формата", $"Не удалось разобрать JSON:\n{ex.Message}", "ОК");
                return;
            }

            if (words is null || words.Count == 0)
            {
                await CustomAlertPage.ShowAsync("Ошибка", "Файл не содержит слов", "ОК");
                return;
            }

            var importResult = await _db.ImportWordsAsync(words);

            ResultLabel.Text = $"Добавлено: {importResult.AddedCount}. Пропущено: {importResult.SkippedItems.Count}.";
            if (importResult.SkippedItems.Count > 0)
                ResultLabel.Text += "\n\n" + string.Join("\n", importResult.SkippedItems);
        }
        catch (Exception ex)
        {
            await CustomAlertPage.ShowAsync("Ошибка импорта", ex.Message, "ОК");
        }
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        try
        {
            var json = await _db.BuildExportJsonAsync();
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

            var fileSaverResult = await FileSaver.Default.SaveAsync("korean_words.json", stream, CancellationToken.None);

            if (fileSaverResult.IsSuccessful)
            {
                await CustomAlertPage.ShowAsync("Готово", $"Файл сохранён:\n{fileSaverResult.FilePath}", "ОК");
            }
            else
            {
                await CustomAlertPage.ShowAsync("Отменено", "Сохранение файла было отменено или не удалось.", "ОК");
            }
        }
        catch (Exception ex)
        {
            await CustomAlertPage.ShowAsync("Ошибка экспорта", ex.Message, "ОК");
        }
    }
}