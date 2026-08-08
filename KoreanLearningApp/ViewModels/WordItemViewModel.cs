using CommunityToolkit.Mvvm.ComponentModel;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.ViewModels;

public partial class WordItemViewModel(Word word) : ObservableObject
{
    public Word Word { get; } = word;

    public string Korean => Word.Korean;

    public string TopikLevel => Word.TopikLevel;

    public bool HasTopikLevel => !string.IsNullOrWhiteSpace(TopikLevel);

    [ObservableProperty]
    private string _translationRu = word.KrDict?.Senses.FirstOrDefault()?.Ru?.Word ?? string.Empty;

    [ObservableProperty]
    private PartOfSpeechEnum _partOfSpeech = PartOfSpeechMapper.Parse(word.PartOfSpeech);

    public string PartOfSpeechDisplay => PartOfSpeech.ToRussian();

    partial void OnPartOfSpeechChanged(PartOfSpeechEnum value)
    {
        OnPropertyChanged(nameof(PartOfSpeechDisplay));
    }
}