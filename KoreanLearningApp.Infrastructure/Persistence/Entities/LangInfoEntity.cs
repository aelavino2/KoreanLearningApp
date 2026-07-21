using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("LangInfos")]
public class LangInfoEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Word { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
}
