using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Abstractions;
using KoreanLearningApp.Infrastructure.Persistence.Abstractions.Repositories;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapping;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordRepository : ImportExportRepository<WordEntity, WordImportDto>, IWordRepository
{
    private readonly IBackupService _backupService;

    public WordRepository(DbContext dbContext, IBackupService backupService)
        : base(dbContext)
    {
        _backupService = backupService;
    }

    public async Task<List<Word>> GetWordsAsync()
    {
        var entities = await GetAllEntitiesAsync();
        return entities.ToDomain();
    }

    public async Task<List<Word>> GetDueWordsAsync(int desiredCount)
    {
        var all = await GetWordsAsync();
        var due = all.Where(SpacedRepetitionHelper.IsDue).ToList();

        if (due.Count < desiredCount)
        {
            var extra = all.Except(due)
                .OrderBy(w => w.NextReviewAt)
                .Take(desiredCount - due.Count);
            due.AddRange(extra);
        }

        return due;
    }

    public async Task<int> SaveWordAsync(Word word)
    {
        var entity = word.ToEntity();
        var result = await SaveAsync(entity);

        if (word.Id == 0)
            word.Id = entity.Id;

        await SaveBackupAsync();
        return result;
    }

    public async Task<ImportResult> ImportWordsAsync(List<WordImportDto> incoming)
    {
        var result = await ImportAsync(incoming);

        if (result.AddedCount > 0)
            await SaveBackupAsync();

        return result;
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        var result = await DeleteAsync(word.ToEntity());
        await SaveBackupAsync();
        return result;
    }

    public async Task<string> BuildExportJsonAsync()
    {
        var words = await GetWordsAsync();
        return _backupService.BuildExportJson(words);
    }

    private async Task SaveBackupAsync()
    {
        var entities = await GetAllEntitiesAsync();
        await _backupService.SaveAsync(entities.ToDomain());
    }

    private static string MakeKey(string korean, string translationRu) =>
        $"{korean.Trim().ToLowerInvariant()}|{translationRu.Trim().ToLowerInvariant()}";

    protected override bool IsValid(WordImportDto dto) =>
        !string.IsNullOrWhiteSpace(dto.Korean) && !string.IsNullOrWhiteSpace(dto.TranslationRu);

    protected override string GetKey(WordImportDto dto) =>
        MakeKey(dto.Korean, dto.TranslationRu);

    protected override string GetKey(WordEntity entity) =>
        MakeKey(entity.Korean, entity.TranslationRu);

    protected override WordEntity MapToEntity(WordImportDto dto) => new()
    {
        Korean = dto.Korean.Trim(),
        TranscriptionRu = dto.TranscriptionRu.Trim(),
        TranscriptionEn = dto.TranscriptionEn.Trim(),
        TranslationRu = dto.TranslationRu.Trim(),
        TranslationEn = dto.TranslationEn.Trim(),
        RuleExplanation = dto.Rule?.Trim() ?? string.Empty,
        Category = dto.Category?.Trim() ?? string.Empty,
        Type = WordTypeHelper.FromStringKey(dto.Type),
        Status = LearningStatus.Learning,
        PronunciationNote = dto.PronunciationNote?.Trim() ?? string.Empty,
        LeitnerBox = 1,
        NextReviewAt = DateTime.UtcNow
    };

    protected override string InvalidMessage(WordImportDto dto) =>
        $"{dto.Korean} — пропущено (нет корейского слова или перевода)";

    protected override string DuplicateInBatchMessage(WordImportDto dto) =>
        $"{dto.Korean} ({dto.TranslationRu}) — повтор внутри присланного списка";

    protected override string AlreadyExistsMessage(WordImportDto dto) =>
        $"{dto.Korean} ({dto.TranslationRu}) — уже есть в базе";
}