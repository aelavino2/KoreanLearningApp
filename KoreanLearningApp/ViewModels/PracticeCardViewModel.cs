using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;
using System.Collections.ObjectModel;


namespace KoreanLearningApp.ViewModels;

public partial class PracticeCardViewModel : ObservableObject
{
    public int WordId { get; }
    public string Korean { get; }
    public string Ru { get; }
    public string En { get; }
    public string PartOfSpeechDisplay { get; }

    /// <summary>true — слово ещё не начато (Progress был null), можно показать бейдж "новое"</summary>
    public bool IsNew { get; }

    [ObservableProperty]
    private bool _isTranslationVisible;

    public PracticeCardViewModel(PracticeCard card)
    {
        WordId = card.Word.Id;
        Korean = card.Word.Korean;

        var firstSense = card.Word.KrDict?.Senses.FirstOrDefault();
        Ru = firstSense?.Ru?.Word ?? string.Empty;
        En = firstSense?.En?.Word ?? string.Empty;

        PartOfSpeechDisplay = PartOfSpeechMapper.Parse(card.Word.PartOfSpeech).ToRussian();
        IsNew = card.IsNew;
    }

    [RelayCommand]
    private void ToggleTranslation() => IsTranslationVisible = !IsTranslationVisible;
}