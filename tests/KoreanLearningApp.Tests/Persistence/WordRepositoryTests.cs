using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Tests.Support;

namespace KoreanLearningApp.Tests.Persistence;

public sealed class WordRepositoryTests : IAsyncLifetime
{
    private readonly SqliteDatabase _database = new();
    private readonly TestServices _services;

    public WordRepositoryTests()
    {
        _services = new TestServices(_database.Context);
    }

    [Fact]
    public async Task SaveWordAsync_WordWithSensesAndAudio_ReadsBackFullGraph()
    {
        var word = TestWords.Create("사과", ru: "яблоко", en: "apple");
        word.KrDict!.Senses.Add(TestWords.CreateSense("извинение", "apology"));
        word.KrDict.Audio = new Audio { Url = "https://example.test/apple.mp3" };

        var id = await _services.WordRepository.SaveWordAsync(word);

        var saved = Assert.Single(await _services.WordRepository.GetByIdsAsync([id]));
        Assert.Equal("사과", saved.Korean);
        Assert.Equal(["яблоко", "извинение"], saved.KrDict!.Senses.Select(s => s.Ru!.Word));
        Assert.Equal(["apple", "apology"], saved.KrDict.Senses.Select(s => s.En!.Word));
        Assert.Equal("https://example.test/apple.mp3", saved.KrDict.Audio!.Url);
    }

    [Fact]
    public async Task SaveWordAsync_ExistingWord_ReplacesChildRowsWithoutOrphans()
    {
        var word = TestWords.Create("사과", ru: "яблоко", en: "apple");
        word.Id = await _services.WordRepository.SaveWordAsync(word);
        word.KrDict!.Senses = [TestWords.CreateSense("яблочко", "small apple")];

        await _services.WordRepository.SaveWordAsync(word);

        var saved = Assert.Single(await _services.WordRepository.GetWordsAsync());
        Assert.Equal("яблочко", Assert.Single(saved.KrDict!.Senses).Ru!.Word);
        Assert.Equal(1, await _database.CountAsync("KrDicts"));
        Assert.Equal(1, await _database.CountAsync("KrDictSenses"));
        Assert.Equal(2, await _database.CountAsync("LangInfos"));
    }

    [Fact]
    public async Task DeleteWordAsync_SavedWord_LeavesNoOrphanRows()
    {
        var word = TestWords.Create("사과", ru: "яблоко", en: "apple");
        word.KrDict!.Audio = new Audio { File = "audio/1.mp3" };
        word.Id = await _services.WordRepository.SaveWordAsync(word);

        await _services.WordRepository.DeleteWordAsync(word);

        foreach (var table in new[] { "Words", "KrDicts", "KrDictSenses", "LangInfos", "Audios" })
            Assert.Equal(0, await _database.CountAsync(table));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _database.DisposeAsync();
}
