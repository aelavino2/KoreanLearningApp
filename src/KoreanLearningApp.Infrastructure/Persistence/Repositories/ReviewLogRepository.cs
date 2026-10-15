using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public sealed class ReviewLogRepository(IRepository<ReviewLogEntity> logRepo) : IReviewLogRepository
{
    public Task AddAsync(ReviewLog log) => logRepo.InsertAsync(log.ToEntity());
}
