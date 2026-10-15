using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Presentation.Dialogs;
using KoreanLearningApp.Presentation.Navigation;
using KoreanLearningApp.Presentation.ViewModels;
using KoreanLearningApp.Services.Abstractions.Services;
using KoreanLearningApp.Tests.Support;
using NSubstitute;

namespace KoreanLearningApp.Tests.ViewModels;

public class WordDetailViewModelTests
{
    private readonly WordDetailViewModel _viewModel = new(
        Substitute.For<INavigationService>(),
        Substitute.For<IAudioPlayerService>(),
        Substitute.For<ISavedWordsService>(),
        Substitute.For<IDialogService>());

    [Theory]
    [InlineData("apple", "яблоко", "apple")]
    [InlineData(null, "яблоко", "яблоко")]
    public void PrimaryTranslation_FirstSense_PrefersEnglishThenRussian(string? en, string ru, string expected)
    {
        _viewModel.Word = TestWords.Create("사과", ru: ru, en: en);

        Assert.Equal(expected, _viewModel.PrimaryTranslation);
    }

    [Theory]
    [InlineData("audio/1.mp3", true)]
    [InlineData("", false)]
    public void PlayAudioCommand_DependsOnWordAudio(string audioFile, bool expectedCanExecute)
    {
        var word = TestWords.Create("사과", id: 1);
        word.KrDict!.Audio = new Audio { File = audioFile };

        _viewModel.Word = word;

        Assert.Equal(expectedCanExecute, _viewModel.PlayAudioCommand.CanExecute(null));
    }
}
