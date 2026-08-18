using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("ReviewLogs")]
public class ReviewLogEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int WordId { get; set; }

    public DateTime ReviewedAt { get; set; }
    
    public int Rating { get; set; }

    public int IntervalBefore { get; set; }
    public int IntervalAfter { get; set; }

    public double EasinessBefore { get; set; }
    public double EasinessAfter { get; set; }
}