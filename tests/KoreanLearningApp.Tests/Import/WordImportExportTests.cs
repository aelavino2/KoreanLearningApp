using KoreanLearningApp.Tests.Support;

namespace KoreanLearningApp.Tests.Import;

public sealed class WordImportExportTests : IAsyncLifetime
{
    private const string AppleJson = """
        [{ "word": "사과", "rank": "1", "krdict": { "word": "사과", "sup_no": 1,
           "senses": [{ "ru": { "word": "яблоко" }, "en": { "word": "apple" } }] } }]
        """;

    private readonly SqliteDatabase _database = new();
    private readonly TestServices _services;

    public WordImportExportTests()
    {
        _services = new TestServices(_database.Context);
    }

    [Fact]
    public async Task ImportFromJsonAsync_SameJsonTwice_ImportsOnlyOnce()
    {
        var first = await _services.ImportService.ImportFromJsonAsync(AppleJson);
        var second = await _services.ImportService.ImportFromJsonAsync(AppleJson);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
    }

    [Fact]
    public async Task ImportFromJsonAsync_HomonymsWithDifferentSupNo_ImportsBoth()
    {
        const string json = """
            [{ "word": "배", "krdict": { "word": "배", "sup_no": 1 } },
             { "word": "배", "krdict": { "word": "배", "sup_no": 2 } }]
            """;

        var imported = await _services.ImportService.ImportFromJsonAsync(json);

        Assert.Equal(2, imported);
    }

    [Fact]
    public async Task ImportFromJsonAsync_ConcatenatedRootObjects_ImportsAll()
    {
        const string json = """
            { "word": "하나", "krdict": { "word": "하나", "sup_no": 1 } }
            { "word": "둘", "krdict": { "word": "둘", "sup_no": 1 } }
            """;

        var imported = await _services.ImportService.ImportFromJsonAsync(json);

        Assert.Equal(2, imported);
    }

    [Fact]
    public async Task ImportFromJsonAsync_RawWordAndInvalidRank_UsesKrDictWordAndZeroRank()
    {
        const string json = """[{ "word": "사과01", "rank": "n/a", "krdict": { "word": "사과", "sup_no": 1 } }]""";

        await _services.ImportService.ImportFromJsonAsync(json);

        var word = Assert.Single(await _services.WordRepository.GetWordsAsync());
        Assert.Equal("사과", word.Korean);
        Assert.Equal(0, word.Rank);
    }

    [Fact]
    public async Task ExportToJsonAsync_ImportIntoEmptyDatabase_PreservesWord()
    {
        await _services.ImportService.ImportFromJsonAsync(AppleJson);
        var json = await _services.ExportService.ExportToJsonAsync(await _services.WordRepository.GetWordsAsync());
        await using var targetDatabase = new SqliteDatabase();
        var target = new TestServices(targetDatabase.Context);

        await target.ImportService.ImportFromJsonAsync(json);

        var word = Assert.Single(await target.WordRepository.GetWordsAsync());
        var sense = Assert.Single(word.KrDict!.Senses);
        Assert.Equal(("사과", 1, 1), (word.Korean, word.Rank, word.KrDict.SupNo));
        Assert.Equal(("яблоко", "apple"), (sense.Ru!.Word, sense.En!.Word));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _database.DisposeAsync();
}
