using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class PracticeViewModel(IWordPracticeService practiceService, INavigationService navigationService)
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
            var practiceCards = await practiceService.GetPracticeSessionAsync();

            Cards.Clear();
            foreach (var card in practiceCards)
                Cards.Add(new PracticeCardViewModel(card));

            IsEmpty = Cards.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task MarkKnown(PracticeCardViewModel card)
    {
        if (card is null)
            return;

        await practiceService.SubmitReviewAsync(card.WordId, ReviewRatingEnum.Good);

        Cards.Remove(card);
        IsEmpty = Cards.Count == 0;
    }

    [RelayCommand]
    private async Task MarkUnknown(PracticeCardViewModel card)
    {
        if (card is null)
            return;

        await practiceService.SubmitReviewAsync(card.WordId, ReviewRatingEnum.Forgot);

        Cards.Remove(card);
        IsEmpty = Cards.Count == 0;
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