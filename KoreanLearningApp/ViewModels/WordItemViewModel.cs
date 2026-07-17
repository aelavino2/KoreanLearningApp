using CommunityToolkit.Mvvm.ComponentModel;
using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.ViewModels;
public partial class WordItemViewModel : ObservableObject
{
    public Word Word { get; }

    public WordItemViewModel(Word word)
    {
        Word = word;
        _translationRu = word.TranslationRu;
    }

    public string Korean => Word.Korean;

    [ObservableProperty]
    private string _translationRu;
}