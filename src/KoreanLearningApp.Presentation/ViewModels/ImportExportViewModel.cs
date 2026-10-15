using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Presentation.Dialogs;
using KoreanLearningApp.Presentation.Files;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Presentation.ViewModels;

public partial class ImportExportViewModel(
    IWordService wordService,
    IWordImportService wordImportService,
    IWordExportService wordExportService,
    IFileService fileService,
    IDialogService dialogService,
    TimeProvider timeProvider) : ObservableObject
{
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var words = await wordService.GetWordsAsync();
            var json = await wordExportService.ExportToJsonAsync(words);
            var fileName = $"korean_words_{timeProvider.GetLocalNow():yyyyMMdd_HHmmss}.json";

            await fileService.ShareTextFileAsync(fileName, json, "Экспорт слов");
            await dialogService.ShowAlertAsync("Экспорт", "Слова успешно экспортированы");
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось экспортировать: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            var json = await fileService.PickJsonFileAsync("Выберите JSON файл");
            if (json is null)
                return;

            var importedCount = await wordImportService.ImportFromJsonAsync(json);

            var message = importedCount > 0
                ? $"Успешно импортировано слов: {importedCount}"
                : "Новых слов для импорта не найдено (возможно, все уже есть в словаре)";

            await dialogService.ShowAlertAsync("Импорт", message);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось импортировать: {ex.Message}");
        }
    }
}
