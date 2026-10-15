using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
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

            var result = await wordImportService.ImportFromJsonAsync(json);
            await dialogService.ShowAlertAsync("Импорт", BuildImportMessage(result));
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось импортировать: {ex.Message}");
        }
    }

    private static string BuildImportMessage(WordImportResult result)
    {
        if (result is { AddedWords: 0, TaggedExistingWords: 0 })
            return "Новых слов и тегов не найдено (возможно, всё уже есть в словаре)";

        var parts = new List<string>();
        if (result.AddedWords > 0)
            parts.Add($"Добавлено слов: {result.AddedWords}");
        if (result.TaggedExistingWords > 0)
            parts.Add($"Обновлены теги у слов: {result.TaggedExistingWords}");

        return string.Join(Environment.NewLine, parts);
    }
}
