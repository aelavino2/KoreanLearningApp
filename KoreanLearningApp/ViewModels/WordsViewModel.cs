using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;

namespace KoreanLearningApp.ViewModels;

public partial class WordsViewModel : ObservableObject
{
    private readonly IWordService _wordService;
    private readonly INavigationService _navigationService;
    private List<WordItemViewModel> _allWords = new();

    public ObservableCollection<WordItemViewModel> Words { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    public WordsViewModel(IWordService wordService, INavigationService navigationService)
    {
        _wordService = wordService;
        _navigationService = navigationService;
    }

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

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allWords
            : _allWords.Where(w =>
                w.Korean.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                w.TranslationRu.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Words.Clear();
        foreach (var word in filtered)
            Words.Add(word);
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