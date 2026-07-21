using CommunityToolkit.Mvvm.ComponentModel;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.ViewModels;

public partial class WordItemViewModel : ObservableObject
{
    public Word Word { get; }

    public WordItemViewModel(Word word)
    {
        Word = word;
        _translationRu = word.TranslationRu;
        _partOfSpeech = PartOfSpeechMapper.Parse(word.PartOfSpeech);
    }

    public string Korean => Word.Korean;

    [ObservableProperty]
    private string _translationRu;

    [ObservableProperty]
    private PartOfSpeechEnum _partOfSpeech;
    
    public string PartOfSpeechDisplay => PartOfSpeech.ToRussian();

    partial void OnPartOfSpeechChanged(PartOfSpeechEnum value)
    {
        OnPropertyChanged(nameof(PartOfSpeechDisplay));
    }
}