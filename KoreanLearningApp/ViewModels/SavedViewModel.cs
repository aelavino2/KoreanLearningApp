using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Dialogs;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public partial class SavedViewModel(
    ISavedWordsService savedWordsService,
    INavigationService navigationService,
    IDialogService dialogService) : ObservableObject
{
    public ObservableCollection<WordItemViewModel> SavedWords { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [RelayCommand]
    private async Task LoadSavedAsync()
    {
        IsLoading = true;
        try
        {
            var words = await savedWordsService.GetSavedWordsAsync();

            SavedWords.Clear();
            foreach (var word in words)
                SavedWords.Add(new WordItemViewModel(word));

            IsEmpty = SavedWords.Count == 0;
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось загрузить сохранённые слова: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveFromSavedAsync(WordItemViewModel? item)
    {
        if (item is null)
            return;

        try
        {
            await savedWordsService.RemoveAsync(item.Word.Id);
            SavedWords.Remove(item);
            IsEmpty = SavedWords.Count == 0;
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось удалить слово: {ex.Message}");
        }
    }

    [RelayCommand]
    private Task OpenWordDetailAsync(WordItemViewModel? item) =>
        item is null ? Task.CompletedTask : navigationService.GoToDetailAsync(AppRoutes.WordDetail, item.Word);
}
