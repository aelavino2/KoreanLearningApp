using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Navigation;

namespace KoreanLearningApp.ViewModels;

[QueryProperty(nameof(Word), "Word")]
public partial class WordDetailViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public WordDetailViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private Word _word = new();

    partial void OnWordChanged(Word value)
    {
        Senses = value.Senses
            .Select(s => new SenseItemViewModel(s))
            .ToList();

        OnPropertyChanged(nameof(PrimaryTranslation));
        OnPropertyChanged(nameof(PartOfSpeechDisplay));
    }

    [ObservableProperty]
    private List<SenseItemViewModel> _senses = new();

    public string PrimaryTranslation =>
        !string.IsNullOrWhiteSpace(Word.TranslationEn)
            ? Word.TranslationEn
            : Word.TranslationRu;

    public string PartOfSpeechDisplay => Word.PartOfSpeech;

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoBackAsync();
}