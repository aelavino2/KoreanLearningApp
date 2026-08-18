using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Domain.Models;

public class ReviewLog
{
    public int Id { get; set; }
    public int WordId { get; set; }

    public DateTime ReviewedAt { get; set; }
    public ReviewRatingEnum Rating { get; set; }

    public int IntervalBefore { get; set; }
    public int IntervalAfter { get; set; }

    public double EasinessBefore { get; set; }
    public double EasinessAfter { get; set; }
}