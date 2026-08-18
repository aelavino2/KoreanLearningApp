using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

[QueryProperty(nameof(Word), "Word")]
public partial class WordDetailViewModel(
    INavigationService navigationService, IAudioPlayerService audioPlayerService,
    ISavedWordsService savedWordsService)
    : ObservableObject
{
    [ObservableProperty]
    private Word _word = new();

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    private bool _isSaveBusy;

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

        _ = LoadSavedStateAsync(value.Id);
    }

    private async Task LoadSavedStateAsync(int wordId)
    {
        if (wordId == 0)
        {
            IsSaved = false;
            return;
        }

        IsSaved = await savedWordsService.IsSavedAsync(wordId);
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

    [RelayCommand]
    private async Task ToggleSaveAsync()
    {
        if (IsSaveBusy || Word.Id == 0)
            return;

        try
        {
            IsSaveBusy = true;
            IsSaved = await savedWordsService.ToggleAsync(Word.Id);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить слово: {ex.Message}", "OK");
        }
        finally
        {
            IsSaveBusy = false;
        }
    }

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