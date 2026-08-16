using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class SavedViewModel(ISavedWordsService savedWordsService, INavigationService navigationService)
    : ObservableObject
{
    public ObservableCollection<SavedWordItemViewModel> SavedWords { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [RelayCommand]
    public async Task LoadSavedAsync()
    {
        IsLoading = true;
        try
        {
            var words = await savedWordsService.GetSavedWordsAsync();

            SavedWords.Clear();
            foreach (var word in words)
                SavedWords.Add(new SavedWordItemViewModel(word));

            IsEmpty = SavedWords.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveFromSaved(SavedWordItemViewModel item)
    {
        if (item is null)
            return;

        await savedWordsService.RemoveAsync(item.WordId);

        SavedWords.Remove(item);
        IsEmpty = SavedWords.Count == 0;
    }

    [RelayCommand]
    private Task GoToWordsAsync() => navigationService.GoToRootAsync(AppRoutes.Words);

    [RelayCommand]
    private Task GoToPracticeAsync() => navigationService.GoToRootAsync(AppRoutes.Practice);

    [RelayCommand]
    private Task GoToImportExportAsync() => navigationService.GoToRootAsync(AppRoutes.ImportExport);

    [RelayCommand]
    private Task GoToSavedAsync() => navigationService.GoToRootAsync(AppRoutes.Saved);
}