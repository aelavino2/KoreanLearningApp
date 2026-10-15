using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Presentation.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.Presentation.ViewModels;

public partial class WordsViewModel(IWordService wordService, INavigationService navigationService)
    : ObservableObject
{
    private const int PageSize = 100;
    private const int SearchDebounceMs = 300;
    private const int InfiniteScrollThreshold = 10;

    private CancellationTokenSource? _searchDebounceCts;
    private bool _reachedEndOfCurrentPage;

    /// <summary>
    /// Список заменён новой страницей — View должна прокрутить его в начало.
    /// </summary>
    public event EventHandler? ScrollToTopRequested;

    public ObservableCollection<WordItemViewModel> Words { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageIndicatorText))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageIndicatorText), nameof(IsPaginationVisible))]
    private int _totalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoadedWordsText))]
    private int _totalCount;

    [ObservableProperty]
    private bool _canGoToNextPage;

    [ObservableProperty]
    private bool _canGoToPreviousPage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPaginationVisible), nameof(RemainingItemsThreshold), nameof(LoadedWordsText))]
    private bool _isInfiniteScroll;

    [ObservableProperty]
    private bool _isLoading;

    public string PageIndicatorText => $"{CurrentPage} / {TotalPages}";

    public bool IsPaginationVisible => !IsInfiniteScroll && TotalPages > 1;

    public int RemainingItemsThreshold => IsInfiniteScroll ? InfiniteScrollThreshold : 0;

    public string LoadedWordsText => IsInfiniteScroll
        ? $"Загружено {Words.Count} из {TotalCount}"
        : $"Слов на странице: {Words.Count}";

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

        await LoadWordsAsync();
    }

    [RelayCommand]
    public async Task LoadWordsAsync()
    {
        CurrentPage = 1;
        _reachedEndOfCurrentPage = false;
        await LoadCurrentPageAsync(append: false);
    }

    [RelayCommand]
    private Task UsePagingAsync() => SwitchModeAsync(infiniteScroll: false);

    [RelayCommand]
    private Task UseInfiniteScrollAsync() => SwitchModeAsync(infiniteScroll: true);

    private async Task SwitchModeAsync(bool infiniteScroll)
    {
        if (IsInfiniteScroll == infiniteScroll)
            return;

        IsInfiniteScroll = infiniteScroll;
        await LoadWordsAsync();
    }

    private async Task LoadCurrentPageAsync(bool append)
    {
        IsLoading = true;
        try
        {
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
            var (items, totalCount) = await wordService.GetWordsPageAsync(CurrentPage, PageSize, search);

            if (!append)
                Words.Clear();

            foreach (var word in items)
                Words.Add(new WordItemViewModel(word));

            TotalCount = totalCount;
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            OnPropertyChanged(nameof(LoadedWordsText));
            UpdatePaginationState();

            if (!append)
                ScrollToTopRequested?.Invoke(this, EventArgs.Empty);
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
    private async Task ScrolledToEndAsync()
    {
        if (IsInfiniteScroll)
        {
            await LoadNextChunkAsync();
            return;
        }

        if (_reachedEndOfCurrentPage)
            return;

        _reachedEndOfCurrentPage = true;
        UpdatePaginationState();
    }

    private async Task LoadNextChunkAsync()
    {
        if (IsLoading || CurrentPage >= TotalPages)
            return;

        CurrentPage++;
        await LoadCurrentPageAsync(append: true);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!CanGoToNextPage)
            return;

        CurrentPage++;
        _reachedEndOfCurrentPage = false;
        await LoadCurrentPageAsync(append: false);
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (!CanGoToPreviousPage)
            return;

        CurrentPage--;
        _reachedEndOfCurrentPage = true;
        await LoadCurrentPageAsync(append: false);
    }
}
