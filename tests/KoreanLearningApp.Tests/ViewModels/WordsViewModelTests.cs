using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Presentation.Navigation;
using KoreanLearningApp.Presentation.ViewModels;
using KoreanLearningApp.Services.Abstractions.Services;
using KoreanLearningApp.Tests.Support;
using NSubstitute;

namespace KoreanLearningApp.Tests.ViewModels;

public class WordsViewModelTests
{
    private readonly IWordService _wordService = Substitute.For<IWordService>();
    private readonly WordsViewModel _viewModel;

    public WordsViewModelTests()
    {
        _viewModel = new WordsViewModel(_wordService, Substitute.For<INavigationService>());
    }

    [Fact]
    public async Task ScrolledToEnd_SeveralPages_EnablesNextPageOnlyAfterScrolling()
    {
        ReturnPage(totalCount: 250);
        await _viewModel.LoadWordsAsync();
        var enabledBeforeScroll = _viewModel.CanGoToNextPage;

        _viewModel.ScrolledToEndCommand.Execute(null);

        Assert.Equal(3, _viewModel.TotalPages);
        Assert.False(enabledBeforeScroll);
        Assert.True(_viewModel.CanGoToNextPage);
    }

    [Fact]
    public async Task ScrolledToEnd_LastPage_KeepsNextPageDisabled()
    {
        ReturnPage(totalCount: 40);
        await _viewModel.LoadWordsAsync();

        _viewModel.ScrolledToEndCommand.Execute(null);

        Assert.False(_viewModel.IsPaginationVisible);
        Assert.False(_viewModel.CanGoToNextPage);
    }

    [Fact]
    public async Task ScrolledToEnd_InfiniteScroll_AppendsNextChunkAndHidesPagination()
    {
        ReturnPage(totalCount: 250);
        await _viewModel.UseInfiniteScrollCommand.ExecuteAsync(null);

        await _viewModel.ScrolledToEndCommand.ExecuteAsync(null);

        Assert.Equal(2, _viewModel.Words.Count);
        Assert.Equal(2, _viewModel.CurrentPage);
        Assert.False(_viewModel.IsPaginationVisible);
    }

    [Fact]
    public async Task NextPage_PagingMode_ReplacesWordsAndRequestsScrollToTop()
    {
        ReturnPage(totalCount: 250);
        await _viewModel.LoadWordsAsync();
        await _viewModel.ScrolledToEndCommand.ExecuteAsync(null);
        var scrollRequests = 0;
        _viewModel.ScrollToTopRequested += (_, _) => scrollRequests++;

        await _viewModel.NextPageCommand.ExecuteAsync(null);

        Assert.Single(_viewModel.Words);
        Assert.Equal(2, _viewModel.CurrentPage);
        Assert.Equal(1, scrollRequests);
    }

    private void ReturnPage(int totalCount) =>
        _wordService.GetWordsPageAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>())
            .Returns((new List<Word> { TestWords.Create("사과") }, totalCount));
}
