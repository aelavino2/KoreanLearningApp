using CommunityToolkit.Mvvm.ComponentModel;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Presentation.Formatting;

namespace KoreanLearningApp.Presentation.ViewModels;

public partial class WordItemViewModel(Word word) : ObservableObject
{
    private const int VisibleTagCount = 2;

    public Word Word { get; } = word;

    public IReadOnlyList<string> TagTitles { get; } = word.Tags
        .Take(VisibleTagCount)
        .Select(t => t.ToDisplayName())
        .ToList();

    public bool HasTags => TagTitles.Count > 0;

    public string Korean => Word.Korean;

    public string TopikLevel => Word.TopikLevel;

    public bool HasTopikLevel => !string.IsNullOrWhiteSpace(TopikLevel);

    [ObservableProperty]
    private string _translationRu = word.KrDict?.Senses.FirstOrDefault()?.Ru?.Word ?? string.Empty;

    [ObservableProperty]
    private PartOfSpeechEnum _partOfSpeech = word.PartOfSpeech;

    public string PartOfSpeechDisplay => PartOfSpeech.ToRussian();

    partial void OnPartOfSpeechChanged(PartOfSpeechEnum value)
    {
        OnPropertyChanged(nameof(PartOfSpeechDisplay));
    }
}