using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Tests.Support;

namespace KoreanLearningApp.Tests.Persistence;

public sealed class WordTagTests : IAsyncLifetime
{
    private static readonly Tag Home = TestWords.Tag("집", "Дом");
    private static readonly Tag Street = TestWords.Tag("길", "Улица");

    private readonly SqliteDatabase _database = new();
    private readonly TestServices _services;

    public WordTagTests()
    {
        _services = new TestServices(_database.Context);
    }

    [Fact]
    public async Task SaveWordAsync_WordsWithSharedTags_StoresTagOnceAndCountsUsage()
    {
        var roomId = await _services.WordRepository.SaveWordAsync(TestWords.Create("방").WithTags(Home, Street));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("문").WithTags(Home));

        var room = Assert.Single(await _services.WordRepository.GetByIdsAsync([roomId]));
        var usage = await _services.TagRepository.GetAllWithUsageAsync();

        Assert.Equal(["Дом", "Улица"], room.Tags.Select(t => t.NameRu));
        Assert.Equal([("Дом", 2), ("Улица", 1)], usage.Select(u => (u.Tag.NameRu, u.WordCount)));
    }

    [Fact]
    public async Task DeleteWordAsync_TaggedWord_RemovesWordTagLinks()
    {
        var word = TestWords.Create("방").WithTags(Home);
        word.Id = await _services.WordRepository.SaveWordAsync(word);

        await _services.WordRepository.DeleteWordAsync(word);

        Assert.Equal(0, await _database.CountAsync("WordTags"));
    }

    [Fact]
    public async Task GetWordsPageAsync_TagFilter_ReturnsWordsWithAnySelectedTag()
    {
        await _services.WordRepository.SaveWordAsync(TestWords.Create("방", rank: 1).WithTags(Home));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("길", rank: 2).WithTags(Street));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("물", rank: 3));
        var tagIds = (await _services.TagRepository.GetAllWithUsageAsync()).Select(u => u.Tag.Id).ToList();

        var (items, totalCount) = await _services.WordRepository.GetWordsPageAsync(new WordsPageRequest(1, 10, TagIds: tagIds));

        Assert.Equal(["방", "길"], items.Select(w => w.Korean));
        Assert.Equal(2, totalCount);
    }

    [Fact]
    public async Task GetWordsPageAsync_SortByTag_GroupsByTagNameAndPutsUntaggedLast()
    {
        await _services.WordRepository.SaveWordAsync(TestWords.Create("물", rank: 0));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("길", rank: 1).WithTags(Street));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("방", rank: 2).WithTags(Home));

        var (items, _) = await _services.WordRepository.GetWordsPageAsync(new WordsPageRequest(1, 10, Sort: WordSortEnum.Tag));

        Assert.Equal(["방", "길", "물"], items.Select(w => w.Korean));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _database.DisposeAsync();
}
