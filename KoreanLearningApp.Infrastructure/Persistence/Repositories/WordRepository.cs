using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence;
using KoreanLearningApp.Infrastructure.Persistence.Abstractions;
using KoreanLearningApp.Infrastructure.Persistence.Abstractions.Repositories;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapping;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordRepository : Repository<WordEntity>, IWordRepository
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
        var existing = await GetAllEntitiesAsync();

        var toInsert = new List<WordEntity>();
        var skipped = new List<string>();
        var seenInBatch = new HashSet<string>();

        foreach (var dto in incoming)
        {
            if (string.IsNullOrWhiteSpace(dto.Korean) || string.IsNullOrWhiteSpace(dto.TranslationRu))
            {
                skipped.Add($"{dto.Korean} — пропущено (нет корейского слова или перевода)");
                continue;
            }

            var key = MakeKey(dto.Korean, dto.TranslationRu);

            if (!seenInBatch.Add(key))
            {
                skipped.Add($"{dto.Korean} ({dto.TranslationRu}) — повтор внутри присланного списка");
                continue;
            }

            if (existing.Any(w => MakeKey(w.Korean, w.TranslationRu) == key))
            {
                skipped.Add($"{dto.Korean} ({dto.TranslationRu}) — уже есть в базе");
                continue;
            }

            toInsert.Add(new WordEntity
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
            });
        }

        if (toInsert.Count > 0)
        {
            await InsertAllAsync(toInsert);
            await SaveBackupAsync();
        }

        return new ImportResult(toInsert.Count, skipped);
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
}