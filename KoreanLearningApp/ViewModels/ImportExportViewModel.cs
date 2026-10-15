using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public partial class ImportExportViewModel(IWordService wordService,
    IWordImportService wordImportService, IWordExportService wordExportService)
    : ObservableObject
{
    private static readonly FilePickerFileType JsonFileType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS, new[] { "public.json" } },
            { DevicePlatform.Android, new[] { "application/json" } },
            { DevicePlatform.WinUI, new[] { ".json" } },
            { DevicePlatform.MacCatalyst, new[] { "json" } },
        });

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var words = await wordService.GetWordsAsync();
            var json = await wordExportService.ExportToJsonAsync(words);

            var fileName = $"korean_words_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, json);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Экспорт слов",
                File = new ShareFile(filePath)
            });

            await Shell.Current.DisplayAlert("Экспорт", "Слова успешно экспортированы", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось экспортировать: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите JSON файл",
                FileTypes = JsonFileType
            });

            if (result is null)
                return;

            using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var importedCount = await wordImportService.ImportFromJsonAsync(json);

            var message = importedCount > 0
                ? $"Успешно импортировано слов: {importedCount}"
                : "Новых слов для импорта не найдено (возможно, все уже есть в словаре)";

            await Shell.Current.DisplayAlert("Импорт", message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось импортировать: {ex.Message}", "OK");
        }
    }
}