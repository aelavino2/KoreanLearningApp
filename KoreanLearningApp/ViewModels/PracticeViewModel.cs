using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class PracticeViewModel(INavigationService navigationService)
    : ObservableObject
{
    public ObservableCollection<PracticeCardViewModel> Cards { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [RelayCommand]
    public async Task LoadPracticeAsync()
    {
        IsLoading = true;
        try
        {
            // TODO: заменить на реальную выборку слов для практики
            await Task.Delay(200);

            Cards.Clear();
            foreach (var mock in GetMockCards())
                Cards.Add(mock);

            IsEmpty = Cards.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static List<PracticeCardViewModel> GetMockCards() =>
    [
        new PracticeCardViewModel
        {
            Korean = "안녕하세요",
            Ru = "Здравствуйте",
            En = "Hello",
            PartOfSpeechDisplay = "Приветствие"
        },
        new PracticeCardViewModel
        {
            Korean = "감사합니다",
            Ru = "Спасибо",
            En = "Thank you",
            PartOfSpeechDisplay = "Приветствие"
        },
        new PracticeCardViewModel
        {
            Korean = "사랑",
            Ru = "Любовь",
            En = "Love",
            PartOfSpeechDisplay = "Существительное"
        }
    ];

    [RelayCommand]
    private void MarkKnown(PracticeCardViewModel card)
    {
        // TODO: заглушка, логика повторения появится позже
        if (card is null)
            return;

        Cards.Remove(card);
        IsEmpty = Cards.Count == 0;
    }

    [RelayCommand]
    private void MarkUnknown(PracticeCardViewModel card)
    {
        // TODO: заглушка, логика повторения появится позже
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

public partial class PracticeCardViewModel : ObservableObject
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
    private bool _isTranslationVisible;

    [RelayCommand]
    private void ToggleTranslation() => IsTranslationVisible = !IsTranslationVisible;
}