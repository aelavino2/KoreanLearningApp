using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("Audios")]
public class AudioEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Url { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
}