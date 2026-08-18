using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class ReviewLogRepository(IRepository<ReviewLogEntity> logRepo) : IReviewLogRepository
{
    public Task AddAsync(ReviewLog log) => logRepo.InsertAsync(log.ToEntity());

    public async Task<List<ReviewLog>> GetByWordIdAsync(int wordId)
    {
        var entities = await logRepo.GetWhereAsync(l => l.WordId == wordId);
        return entities
            .OrderBy(e => e.ReviewedAt)
            .Select(e => e.ToDomain())
            .ToList();
    }
}