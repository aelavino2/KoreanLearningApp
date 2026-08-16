using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("WordProgresses")]
public class WordProgressEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Unique]
    public int WordId { get; set; }
    
    public int LearningStatus { get; set; }

    public double EasinessFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; }
    public int RepetitionCount { get; set; }
    
    [Indexed]
    public DateTime NextReviewDate { get; set; }
    public DateTime? LastReviewedDate { get; set; }

    public int TotalReviews { get; set; }
    public int TotalCorrect { get; set; }
    public int LapseCount { get; set; }

    public DateTime FirstSeenAt { get; set; }
}