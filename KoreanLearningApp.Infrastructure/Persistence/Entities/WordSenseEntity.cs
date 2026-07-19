using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("WordSenses")]
public class WordSenseEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int WordId { get; set; }

    public string DefinitionKo { get; set; } = string.Empty;
    public string EnWord { get; set; } = string.Empty;
    public string EnDefinition { get; set; } = string.Empty;
    public string RuWord { get; set; } = string.Empty;
    public string RuDefinition { get; set; } = string.Empty;
}

