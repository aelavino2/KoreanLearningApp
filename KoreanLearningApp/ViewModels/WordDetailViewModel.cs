using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

[QueryProperty(nameof(Word), "Word")]
public partial class WordDetailViewModel(INavigationService navigationService, IAudioPlayerService audioPlayerService)
    : ObservableObject
{
    [ObservableProperty]
    private Word _word = new();

    partial void OnWordChanged(Word value)
    {
        Senses = (value.KrDict?.Senses ?? new List<Sense>())
            .Select(s => new SenseItemViewModel(s))
            .ToList();

        OnPropertyChanged(nameof(PrimaryTranslation));
        OnPropertyChanged(nameof(PartOfSpeechDisplay));
        OnPropertyChanged(nameof(HasHanja));
        OnPropertyChanged(nameof(HasPronunciation));
        OnPropertyChanged(nameof(HasRuleExplanation));
        OnPropertyChanged(nameof(HasAudio));

        PlayAudioCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private List<SenseItemViewModel> _senses = new();

    private Sense? FirstSense => Word.KrDict?.Senses.FirstOrDefault();

    public string PrimaryTranslation =>
        !string.IsNullOrWhiteSpace(FirstSense?.En?.Word)
            ? FirstSense!.En!.Word
            : FirstSense?.Ru?.Word ?? string.Empty;

    public string PartOfSpeechDisplay => Word.PartOfSpeech;

    public bool HasHanja => !string.IsNullOrWhiteSpace(Word.Hanja);

    public bool HasPronunciation => !string.IsNullOrWhiteSpace(Word.KrDict?.Pronunciation);

    public bool HasRuleExplanation => !string.IsNullOrWhiteSpace(Word.RuleExplanation);

    public bool HasAudio =>
        !string.IsNullOrWhiteSpace(Word.KrDict?.Audio?.Url) ||
        !string.IsNullOrWhiteSpace(Word.KrDict?.Audio?.File);

    [ObservableProperty]
    private bool _isAudioBusy;

    [RelayCommand]
    private Task GoBackAsync() => navigationService.GoBackAsync();

    [RelayCommand(CanExecute = nameof(HasAudio))]
    private async Task PlayAudioAsync()
    {
        if (IsAudioBusy)
            return;

        var audio = Word.KrDict?.Audio;
        if (audio is null)
            return;

        try
        {
            IsAudioBusy = true;
            await audioPlayerService.PlayAsync(audio.Url, audio.File);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось воспроизвести аудио: {ex.Message}", "OK");
        }
        finally
        {
            IsAudioBusy = false;
        }
    }
}