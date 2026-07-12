using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Maui.Storage;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
using KoreanLearningApp.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace KoreanLearningApp.ViewModels;

public class ImportExportPageViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _db;

    private static readonly FilePickerFileType JsonFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".json" } },
        { DevicePlatform.Android, new[] { "application/json" } },
        { DevicePlatform.iOS, new[] { "public.json" } },
        { DevicePlatform.MacCatalyst, new[] { "json" } },
    });

    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }

    public ImportExportPageViewModel(DatabaseService db)
    {
        _db = db;
        ImportCommand = new Command(async () => await OnImportAsync());
        ExportCommand = new Command(async () => await OnExportAsync());
        RefreshBackupPath();
    }

    private string _resultText = string.Empty;
    public string ResultText
    {
        get => _resultText;
        set { _resultText = value; OnPropertyChanged(); }
    }

    private string _backupPathText = string.Empty;
    public string BackupPathText
    {
        get => _backupPathText;
        set { _backupPathText = value; OnPropertyChanged(); }
    }

    public void RefreshBackupPath()
    {
        BackupPathText =
            $"Резервная копия автоматически сохраняется здесь при каждом изменении словаря:\n{DatabaseService.GetBackupFilePath()}";
    }

    private async Task OnImportAsync()
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
            ResultText = $"Добавлено: {importResult.AddedCount}. Пропущено: {importResult.SkippedItems.Count}.";
            if (importResult.SkippedItems.Count > 0)
                ResultText += "\n\n" + string.Join("\n", importResult.SkippedItems);
        }
        catch (Exception ex)
        {
            await CustomAlertPage.ShowAsync("Ошибка импорта", ex.Message, "ОК");
        }
    }

    private async Task OnExportAsync()
    {
        try
        {
            var json = await _db.BuildExportJsonAsync();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}