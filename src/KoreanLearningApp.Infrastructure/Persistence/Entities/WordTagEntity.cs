using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("WordTags")]
public class WordTagEntity
{
    private const string WordTagIndex = "IX_WordTags_WordId_TagId";

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Name = WordTagIndex, Order = 1, Unique = true)]
    public int WordId { get; set; }

    [Indexed(Name = WordTagIndex, Order = 2, Unique = true)]
    public int TagId { get; set; }
}
