namespace KoreanLearningApp.Diagnostics;

/// <summary>
/// Папки данных приложения: на Windows — %LOCALAPPDATA%\koreanApp.
/// </summary>
public static class AppPaths
{
    private const string AppFolderName = "koreanApp";
    private const string LogsFolderName = "logs";

    public static string DataDirectory => EnsureDirectory(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName));

    public static string LogsDirectory => EnsureDirectory(Path.Combine(DataDirectory, LogsFolderName));

    public static string GetDataFilePath(string fileName) => Path.Combine(DataDirectory, fileName);

    private static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}
