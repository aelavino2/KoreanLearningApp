using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("Tags")]
public class TagEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string NameKo { get; set; } = string.Empty;

    public string NameRu { get; set; } = string.Empty;
}
