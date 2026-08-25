using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public partial class NavigationBarViewModel(INavigationService navigationService)
    : ObservableObject
{
    [ObservableProperty]
    private AppTab _activeTab;

    [RelayCommand]
    private Task GoToWordsAsync() => NavigateAsync(AppTab.Words, AppRoutes.Words);

    [RelayCommand]
    private Task GoToPracticeAsync() => NavigateAsync(AppTab.Practice, AppRoutes.Practice);

    [RelayCommand]
    private Task GoToImportExportAsync() => NavigateAsync(AppTab.ImportExport, AppRoutes.ImportExport);

    [RelayCommand]
    private Task GoToSavedAsync() => NavigateAsync(AppTab.Saved, AppRoutes.Saved);

    private async Task NavigateAsync(AppTab tab, string route)
    {
        if (ActiveTab == tab)
            return;

        await navigationService.GoToRootAsync(route);
        ActiveTab = tab;
    }
}