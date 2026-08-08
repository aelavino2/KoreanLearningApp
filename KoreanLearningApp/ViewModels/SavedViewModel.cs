using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class SavedViewModel(INavigationService navigationService)
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
            // TODO: заменить на реальную выборку сохранённых слов,
            // когда появится IWordService.GetSavedWordsAsync() (или аналог)
            await Task.Delay(200);

            SavedWords.Clear();
            foreach (var mock in GetMockSavedWords())
                SavedWords.Add(mock);

            IsEmpty = SavedWords.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static List<SavedWordItemViewModel> GetMockSavedWords() =>
    [
        new SavedWordItemViewModel
        {
            Korean = "안녕하세요",
            Ru = "Здравствуйте",
            En = "Hello",
            PartOfSpeechDisplay = "Приветствие",
            TopikLevel = "1"
        },
        new SavedWordItemViewModel
        {
            Korean = "감사합니다",
            Ru = "Спасибо",
            En = "Thank you",
            PartOfSpeechDisplay = "Приветствие",
            TopikLevel = "1"
        },
        new SavedWordItemViewModel
        {
            Korean = "사랑",
            Ru = "Любовь",
            En = "Love",
            PartOfSpeechDisplay = "Существительное",
            TopikLevel = "2"
        },
        new SavedWordItemViewModel
        {
            Korean = "공부하다",
            Ru = "Учиться",
            En = "To study",
            PartOfSpeechDisplay = "Глагол",
            TopikLevel = "3"
        }
    ];

    [RelayCommand]
    private void RemoveFromSaved(SavedWordItemViewModel item)
    {
        // TODO: заглушка, реальное удаление из сохранённых появится позже,
        // когда будет известна структура хранения "избранного"
        if (item is null)
            return;

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

public partial class SavedWordItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _korean = string.Empty;

    [ObservableProperty]
    private string _ru = string.Empty;

    [ObservableProperty]
    private string _en = string.Empty;

    [ObservableProperty]
    private string _partOfSpeechDisplay = string.Empty;

    [ObservableProperty]
    private string _topikLevel = string.Empty;

    public bool HasTopikLevel => !string.IsNullOrEmpty(TopikLevel);
}