using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class PracticeViewModel(IWordPracticeService practiceService, ISavedWordsService savedWordsService, INavigationService navigationService)
    : ObservableObject
{
    public ObservableCollection<PracticeCardViewModel> Cards { get; } = new();

    /// <summary>Варианты количества карточек для Picker'а на экране настроек.</summary>
    public List<int> WordCountOptions { get; } = new() { 5, 10, 15, 20, 30, 50 };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    /// <summary>false — показан экран настроек, true — идёт сессия с карточками.</summary>
    [ObservableProperty]
    private bool _isSessionActive;

    [ObservableProperty]
    private int _wordCount = 20;

    [ObservableProperty]
    private bool _isTopik1Selected;

    [ObservableProperty]
    private bool _isTopik2Selected;

    [ObservableProperty]
    private bool _isTopik3Selected;

    [ObservableProperty]
    private bool _includeSavedWords = true;

    [RelayCommand]
    private async Task StartPracticeAsync()
    {
        IsLoading = true;
        try
        {
            var options = new PracticeSessionOptions
            {
                WordCount = WordCount,
                TopikLevels = BuildSelectedTopikLevels(),
                IncludeSavedWords = IncludeSavedWords
            };

            var practiceCards = await practiceService.GetPracticeSessionAsync(options);

            // Один batch-запрос статуса "сохранено" на всю сессию вместо N вызовов
            // IsSavedAsync по одному на карточку — см. PracticeCardViewModel.
            var savedIds = await savedWordsService.GetSavedWordIdSetAsync();

            Cards.Clear();
            foreach (var card in practiceCards)
                Cards.Add(new PracticeCardViewModel(card, savedWordsService, savedIds.Contains(card.Word.Id)));

            IsEmpty = Cards.Count == 0;
            IsSessionActive = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Вернуться к экрану настроек, не завершая приложение и не теряя контекст страницы.</summary>
    [RelayCommand]
    private void BackToSetup()
    {
        IsSessionActive = false;
        Cards.Clear();
        IsEmpty = false;
    }

    private List<string> BuildSelectedTopikLevels()
    {
        var levels = new List<string>();
        if (IsTopik1Selected) levels.Add("1");
        if (IsTopik2Selected) levels.Add("2");
        if (IsTopik3Selected) levels.Add("3");
        return levels;
    }

    [RelayCommand]
    private Task MarkKnown(PracticeCardViewModel card) => SubmitAndRemoveAsync(card, ReviewRatingEnum.Good);

    [RelayCommand]
    private Task MarkHard(PracticeCardViewModel card) => SubmitAndRemoveAsync(card, ReviewRatingEnum.Hard);

    [RelayCommand]
    private Task MarkUnknown(PracticeCardViewModel card) => SubmitAndRemoveAsync(card, ReviewRatingEnum.Forgot);

    private async Task SubmitAndRemoveAsync(PracticeCardViewModel card, ReviewRatingEnum rating)
    {
        if (card is null)
            return;

        await practiceService.SubmitReviewAsync(card.WordId, rating);

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