using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("SavedWords")]
public class SavedWordEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Unique]
    public int WordId { get; set; }

    public DateTime SavedAt { get; set; }
}