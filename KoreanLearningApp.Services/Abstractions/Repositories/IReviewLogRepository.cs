using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IReviewLogRepository
{
    Task AddAsync(ReviewLog log);
    Task<List<ReviewLog>> GetByWordIdAsync(int wordId);
}