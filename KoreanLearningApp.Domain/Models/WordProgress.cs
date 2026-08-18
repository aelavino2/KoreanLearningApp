using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Domain.Models;

public class WordProgress
{
    public int Id { get; set; }
    public int WordId { get; set; }

    public LearningStatusEnum LearningStatus { get; set; } = LearningStatusEnum.New;

    public double EasinessFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; }
    public int RepetitionCount { get; set; }

    public DateTime NextReviewDate { get; set; }
    public DateTime? LastReviewedDate { get; set; }

    public int TotalReviews { get; set; }
    public int TotalCorrect { get; set; }
    public int LapseCount { get; set; }

    public DateTime FirstSeenAt { get; set; }
}