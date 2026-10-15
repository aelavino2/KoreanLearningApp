using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Dialogs;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

[QueryProperty(nameof(Word), "Word")]
public partial class WordDetailViewModel(
    INavigationService navigationService,
    IAudioPlayerService audioPlayerService,
    ISavedWordsService savedWordsService,
    IDialogService dialogService) : ObservableObject
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
        ToggleSaveCommand.NotifyCanExecuteChanged();

        _ = LoadSavedStateAsync(value.Id);
    }

    [ObservableProperty]
    private List<SenseItemViewModel> _senses = new();

    [ObservableProperty]
    private bool _isSaved;

    private Sense? FirstSense => Word.KrDict?.Senses.FirstOrDefault();

    public string PrimaryTranslation =>
        !string.IsNullOrWhiteSpace(FirstSense?.En?.Word)
            ? FirstSense!.En!.Word
            : FirstSense?.Ru?.Word ?? string.Empty;

    public string PartOfSpeechDisplay => Word.PartOfSpeech.ToRussian();

    public bool HasHanja => !string.IsNullOrWhiteSpace(Word.Hanja);

    public bool HasPronunciation => !string.IsNullOrWhiteSpace(Word.KrDict?.Pronunciation);

    public bool HasRuleExplanation => !string.IsNullOrWhiteSpace(Word.RuleExplanation);

    public bool HasAudio =>
        !string.IsNullOrWhiteSpace(Word.KrDict?.Audio?.Url) ||
        !string.IsNullOrWhiteSpace(Word.KrDict?.Audio?.File);

    private bool IsPersisted => Word.Id != 0;

    [ObservableProperty]
    private bool _isAudioBusy;

    [RelayCommand]
    private Task GoBackAsync() => navigationService.GoBackAsync();

    [RelayCommand(CanExecute = nameof(IsPersisted))]
    private async Task ToggleSaveAsync()
    {
        try
        {
            IsSaved = await savedWordsService.ToggleAsync(Word.Id);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось сохранить слово: {ex.Message}");
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
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось воспроизвести аудио: {ex.Message}");
        }
        finally
        {
            IsAudioBusy = false;
        }
    }

    private async Task LoadSavedStateAsync(int wordId)
    {
        try
        {
            IsSaved = wordId != 0 && await savedWordsService.IsSavedAsync(wordId);
        }
        catch (Exception ex)
        {
            await dialogService.ShowAlertAsync("Ошибка", $"Не удалось проверить сохранение слова: {ex.Message}");
        }
    }
}
