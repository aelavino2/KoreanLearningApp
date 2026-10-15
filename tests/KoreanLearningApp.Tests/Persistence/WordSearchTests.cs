using KoreanLearningApp.Tests.Support;

namespace KoreanLearningApp.Tests.Persistence;

public sealed class WordSearchTests : IAsyncLifetime
{
    private readonly SqliteDatabase _database = new();
    private readonly TestServices _services;

    public WordSearchTests()
    {
        _services = new TestServices(_database.Context);
    }

    [Theory]
    [InlineData("사과")]
    [InlineData("ябло")]
    [InlineData("appl")]
    public async Task GetWordsPageAsync_SearchByKoreanRussianOrEnglish_FindsOnlyMatchingWord(string search)
    {
        await _services.WordRepository.SaveWordAsync(TestWords.Create("사과", ru: "яблоко", en: "apple", rank: 1));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("바나나", ru: "банан", en: "banana", rank: 2));

        var (items, totalCount) = await _services.WordRepository.GetWordsPageAsync(1, 10, search);

        Assert.Equal("사과", Assert.Single(items).Korean);
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task GetWordsPageAsync_SeveralSensesMatch_ReturnsWordOnce()
    {
        var word = TestWords.Create("사과", ru: "яблоко", rank: 1);
        word.KrDict!.Senses.Add(TestWords.CreateSense("яблоня", null));
        await _services.WordRepository.SaveWordAsync(word);

        var (items, totalCount) = await _services.WordRepository.GetWordsPageAsync(1, 10, "ябло");

        Assert.Single(items);
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task GetWordsPageAsync_SecondPage_ReturnsNextWordsByRank()
    {
        await _services.WordRepository.SaveWordAsync(TestWords.Create("셋", rank: 3));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("하나", rank: 1));
        await _services.WordRepository.SaveWordAsync(TestWords.Create("둘", rank: 2));

        var (items, totalCount) = await _services.WordRepository.GetWordsPageAsync(2, 2, null);

        Assert.Equal("셋", Assert.Single(items).Korean);
        Assert.Equal(3, totalCount);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _database.DisposeAsync();
}
