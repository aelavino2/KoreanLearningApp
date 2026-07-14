using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence;

public static class DatabaseConstants
{
    public const string DatabaseFileName = "korean_learning.db3";
    public const string BackupFileName = "words_backup.json";

    public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
}