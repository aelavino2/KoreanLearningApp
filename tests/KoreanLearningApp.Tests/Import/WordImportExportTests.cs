using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Tests.Support;
using SQLite;

namespace KoreanLearningApp.Tests.Import;

public sealed class WordImportExportTests : IAsyncLifetime
{
    private const string AppleJson = """
        [{ "word": "사과", "rank": "1", "krdict": { "word": "사과", "sup_no": 1, "pos": "명사",
           "senses": [{ "ru": { "word": "яблоко" }, "en": { "word": "apple" } }] } }]
        """;

    private const string AppleTagsJson = """
        [{ "word": "사과", "krdict": { "word": "사과", "sup_no": 1 },
           "tags": [{ "name_ko": "과일", "name_ru": "Фрукты" }, { "name_ko": "음식", "name_ru": "Еда" }] }]
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

        Assert.Equal(1, first.AddedWords);
        Assert.Equal(0, second.AddedWords);
    }

    [Fact]
    public async Task ImportFromJsonAsync_HomonymsWithDifferentSupNo_ImportsBoth()
    {
        const string json = """
            [{ "word": "배", "krdict": { "word": "배", "sup_no": 1 } },
             { "word": "배", "krdict": { "word": "배", "sup_no": 2 } }]
            """;

        var imported = await _services.ImportService.ImportFromJsonAsync(json);

        Assert.Equal(2, imported.AddedWords);
    }

    [Fact]
    public async Task ImportFromJsonAsync_ConcatenatedRootObjects_ImportsAll()
    {
        const string json = """
            { "word": "하나", "krdict": { "word": "하나", "sup_no": 1 } }
            { "word": "둘", "krdict": { "word": "둘", "sup_no": 1 } }
            """;

        var imported = await _services.ImportService.ImportFromJsonAsync(json);

        Assert.Equal(2, imported.AddedWords);
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

    [Theory]
    [InlineData("명사")]
    [InlineData("Noun")]
    public async Task ImportFromJsonAsync_KrDictPosInKoreanOrEnglish_MapsToEnum(string pos)
    {
        var json = $$"""[{ "word": "사과", "krdict": { "word": "사과", "sup_no": 1, "pos": "{{pos}}" } }]""";

        await _services.ImportService.ImportFromJsonAsync(json);

        var word = Assert.Single(await _services.WordRepository.GetWordsAsync());
        Assert.Equal(PartOfSpeechEnum.Noun, word.KrDict!.Pos);
    }

    [Fact]
    public async Task ImportFromJsonAsync_FailureMidImport_LeavesNoPartialRows()
    {
        var db = await _database.Context.GetConnectionAsync();
        await db.ExecuteAsync("DROP TABLE KrDictSenses");

        await Assert.ThrowsAsync<SQLiteException>(() => _services.ImportService.ImportFromJsonAsync(AppleJson));

        Assert.Equal((0, 0, 0), (
            await _database.CountAsync("Words"),
            await _database.CountAsync("KrDicts"),
            await _database.CountAsync("LangInfos")));
    }

    [Fact]
    public async Task ImportFromJsonAsync_ExistingWordWithNewTags_AddsTagsWithoutDuplicatingWord()
    {
        await _services.ImportService.ImportFromJsonAsync(AppleJson);

        var first = await _services.ImportService.ImportFromJsonAsync(AppleTagsJson);
        var second = await _services.ImportService.ImportFromJsonAsync(AppleTagsJson);

        var word = Assert.Single(await _services.WordRepository.GetWordsAsync());
        Assert.Equal(new WordImportResult(0, 1), first);
        Assert.Equal(new WordImportResult(0, 0), second);
        Assert.Equal(["Еда", "Фрукты"], word.Tags.Select(t => t.NameRu));
    }

    [Fact]
    public async Task ExportToJsonAsync_ImportIntoEmptyDatabase_PreservesWordAndTags()
    {
        await _services.ImportService.ImportFromJsonAsync(AppleJson);
        await _services.ImportService.ImportFromJsonAsync(AppleTagsJson);
        var json = await _services.ExportService.ExportToJsonAsync(await _services.WordRepository.GetWordsAsync());
        await using var targetDatabase = new SqliteDatabase();
        var target = new TestServices(targetDatabase.Context);

        await target.ImportService.ImportFromJsonAsync(json);

        var word = Assert.Single(await target.WordRepository.GetWordsAsync());
        var sense = Assert.Single(word.KrDict!.Senses);
        Assert.Equal(("사과", 1, 1, PartOfSpeechEnum.Noun), (word.Korean, word.Rank, word.KrDict.SupNo, word.KrDict.Pos));
        Assert.Equal(("яблоко", "apple"), (sense.Ru!.Word, sense.En!.Word));
        Assert.Equal([("음식", "Еда"), ("과일", "Фрукты")], word.Tags.Select(t => (t.NameKo, t.NameRu)));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _database.DisposeAsync();
}
