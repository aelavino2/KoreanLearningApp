using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public partial class PracticeCardViewModel : ObservableObject
{
    private readonly ISavedWordsService _savedWordsService;

    public int WordId { get; }
    public string Korean { get; }
    public string Ru { get; }
    public string En { get; }
    public string PartOfSpeechDisplay { get; }

    /// <summary>true — слово ещё не начато (Progress был null), можно показать бейдж "новое"</summary>
    public bool IsNew { get; }

    [ObservableProperty]
    private bool _isTranslationVisible;

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    private bool _isSaveBusy;

    /// <param name="card">Карточка слова + прогресс из сессии практики.</param>
    /// <param name="savedWordsService">Сервис сохранённых слов — нужен для тоггла звёздочки.</param>
    /// <param name="isSavedInitially">
    /// Статус сохранения, посчитанный один раз batch-запросом в PracticeViewModel
    /// (через GetSavedWordIdSetAsync), а не отдельным IsSavedAsync на каждую карточку —
    /// иначе при 20-25 карточках в сессии это N лишних обращений к БД на каждый LoadPracticeAsync.
    /// </param>
    public PracticeCardViewModel(PracticeCard card, ISavedWordsService savedWordsService, bool isSavedInitially)
    {
        _savedWordsService = savedWordsService;

        WordId = card.Word.Id;
        Korean = card.Word.Korean;

        var firstSense = card.Word.KrDict?.Senses.FirstOrDefault();
        Ru = firstSense?.Ru?.Word ?? string.Empty;
        En = firstSense?.En?.Word ?? string.Empty;

        PartOfSpeechDisplay = PartOfSpeechMapper.Parse(card.Word.PartOfSpeech).ToRussian();
        IsNew = card.IsNew;
        IsSaved = isSavedInitially;
    }

    [RelayCommand]
    private void ToggleTranslation() => IsTranslationVisible = !IsTranslationVisible;

    [RelayCommand]
    private async Task ToggleSaveAsync()
    {
        if (IsSaveBusy)
            return;

        try
        {
            IsSaveBusy = true;
            IsSaved = await _savedWordsService.ToggleAsync(WordId);
        }
        finally
        {
            IsSaveBusy = false;
        }
    }
}