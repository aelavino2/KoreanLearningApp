using System.Collections.ObjectModel;
using System.Windows.Input;
using KoreanLearningApp.Services.Abstractions;

namespace KoreanLearningApp.ViewModels;

public class WordsViewModel : BaseViewModel
{
    private readonly IWordService _wordService;

    private List<WordItemViewModel> _allWords = new();
    public ObservableCollection<WordItemViewModel> Words { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilter();
        }
    }

    public ICommand RefreshCommand { get; }

    public WordsViewModel(IWordService wordService)
    {
        _wordService = wordService;
        RefreshCommand = new Command(async () => await LoadWordsAsync());
    }

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
}