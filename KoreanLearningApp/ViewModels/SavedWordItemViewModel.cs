using CommunityToolkit.Mvvm.ComponentModel;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.ViewModels;

public partial class SavedWordItemViewModel : ObservableObject
{
    public int WordId { get; }
    public string Korean { get; }
    public string Ru { get; }
    public string En { get; }
    public string PartOfSpeechDisplay { get; }
    public string TopikLevel { get; }

    public bool HasTopikLevel => !string.IsNullOrEmpty(TopikLevel);

    public SavedWordItemViewModel(Word word)
    {
        WordId = word.Id;
        Korean = word.Korean;
        TopikLevel = word.TopikLevel;

        var firstSense = word.KrDict?.Senses.FirstOrDefault();
        Ru = firstSense?.Ru?.Word ?? string.Empty;
        En = firstSense?.En?.Word ?? string.Empty;

        PartOfSpeechDisplay = PartOfSpeechMapper.Parse(word.PartOfSpeech).ToRussian();
    }
}