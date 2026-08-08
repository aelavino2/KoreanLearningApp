using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class WordsViewModel(IWordService wordService, INavigationService navigationService)
    : ObservableObject
{
    private const int PageSize = 100;
    private const int SearchDebounceMs = 300;

    private CancellationTokenSource? _searchDebounceCts;
    private bool _reachedEndOfCurrentPage;

    public ObservableCollection<WordItemViewModel> Words { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private bool _canGoToNextPage;

    [ObservableProperty]
    private bool _canGoToPreviousPage;

    [ObservableProperty]
    private bool _isPaginationVisible;

    [ObservableProperty]
    private bool _isLoading;

    public string PageIndicatorText => $"{CurrentPage} / {TotalPages}";

    partial void OnCurrentPageChanged(int value) => OnPropertyChanged(nameof(PageIndicatorText));
    partial void OnTotalPagesChanged(int value) => OnPropertyChanged(nameof(PageIndicatorText));

    partial void OnSearchTextChanged(string value)
    {
        _searchDebounceCts?.Cancel();
        var cts = new CancellationTokenSource();
        _searchDebounceCts = cts;

        _ = DebouncedSearchAsync(cts.Token);
    }

    private async Task DebouncedSearchAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(SearchDebounceMs, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (token.IsCancellationRequested)
            return;

        CurrentPage = 1;
        _reachedEndOfCurrentPage = false;
        await LoadCurrentPageAsync();
    }

    [RelayCommand]
    public async Task LoadWordsAsync()
    {
        CurrentPage = 1;
        _reachedEndOfCurrentPage = false;
        await LoadCurrentPageAsync();
    }

    private async Task LoadCurrentPageAsync()
    {
        IsLoading = true;
        try
        {
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
            var (items, totalCount) = await wordService.GetWordsPageAsync(CurrentPage, PageSize, search);

            Words.Clear();
            foreach (var word in items)
                Words.Add(new WordItemViewModel(word));

            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            IsPaginationVisible = TotalPages > 1;

            UpdatePaginationState();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenWordDetailAsync(Word word)
    {
        if (word is null)
            return;

        await navigationService.GoToDetailAsync(AppRoutes.WordDetail, word);
    }

    private void UpdatePaginationState()
    {
        CanGoToPreviousPage = CurrentPage > 1;
        CanGoToNextPage = _reachedEndOfCurrentPage && CurrentPage < TotalPages;
    }

    [RelayCommand]
    private void ScrolledToEnd()
    {
        if (_reachedEndOfCurrentPage)
            return;

        _reachedEndOfCurrentPage = true;
        UpdatePaginationState();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!CanGoToNextPage)
            return;

        CurrentPage++;
        _reachedEndOfCurrentPage = false;
        await LoadCurrentPageAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (!CanGoToPreviousPage)
            return;

        CurrentPage--;
        _reachedEndOfCurrentPage = true;
        await LoadCurrentPageAsync();
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