using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class WordsViewModel : ObservableObject
{
    private const int PageSize = 100;

    private readonly IWordService _wordService;
    private readonly INavigationService _navigationService;

    private List<WordItemViewModel> _allWords = new();
    private List<WordItemViewModel> _filteredWords = new();
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

    public string PageIndicatorText => $"{CurrentPage} / {TotalPages}";

    public WordsViewModel(IWordService wordService, INavigationService navigationService)
    {
        _wordService = wordService;
        _navigationService = navigationService;
    }

    partial void OnCurrentPageChanged(int value) => OnPropertyChanged(nameof(PageIndicatorText));
    partial void OnTotalPagesChanged(int value) => OnPropertyChanged(nameof(PageIndicatorText));

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    public async Task LoadWordsAsync()
    {
        var words = await _wordService.GetWordsAsync();
        _allWords = words.Select(w => new WordItemViewModel(w)).ToList();
        ApplyFilter();
    }

    [RelayCommand]
    private async Task OpenWordDetailAsync(Word word)
    {
        if (word is null)
            return;

        await _navigationService.GoToDetailAsync(AppRoutes.WordDetail, word);
    }

    private void ApplyFilter()
    {
        _filteredWords = string.IsNullOrWhiteSpace(SearchText)
            ? _allWords
            : _allWords.Where(w =>
                w.Korean.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                w.TranslationRu.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
              .ToList();

        TotalPages = Math.Max(1, (int)Math.Ceiling(_filteredWords.Count / (double)PageSize));
        IsPaginationVisible = TotalPages > 1;

        CurrentPage = 1;
        _reachedEndOfCurrentPage = false;

        LoadCurrentPage();
    }

    private void LoadCurrentPage()
    {
        var pageItems = _filteredWords
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);

        Words.Clear();
        foreach (var word in pageItems)
            Words.Add(word);

        UpdatePaginationState();
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
    private void NextPage()
    {
        if (!CanGoToNextPage)
            return;

        CurrentPage++;
        _reachedEndOfCurrentPage = false;
        LoadCurrentPage();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (!CanGoToPreviousPage)
            return;

        CurrentPage--;
        _reachedEndOfCurrentPage = true;
        LoadCurrentPage();
    }

    [RelayCommand]
    private Task GoToWordsAsync() => _navigationService.GoToRootAsync(AppRoutes.Words);

    [RelayCommand]
    private Task GoToPracticeAsync() => _navigationService.GoToRootAsync(AppRoutes.Practice);

    [RelayCommand]
    private Task GoToImportExportAsync() => _navigationService.GoToRootAsync(AppRoutes.ImportExport);

    [RelayCommand]
    private Task GoToSavedAsync() => _navigationService.GoToRootAsync(AppRoutes.Saved);
}