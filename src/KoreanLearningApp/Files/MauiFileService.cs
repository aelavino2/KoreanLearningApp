using KoreanLearningApp.Presentation.Files;

namespace KoreanLearningApp.Files;

public sealed class MauiFileService : IFileService
{
    private static readonly FilePickerFileType JsonFileType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS, ["public.json"] },
            { DevicePlatform.Android, ["application/json"] },
            { DevicePlatform.WinUI, [".json"] },
            { DevicePlatform.MacCatalyst, ["json"] },
        });

    public async Task<string?> PickJsonFileAsync(string title)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = title,
            FileTypes = JsonFileType
        });

        if (result is null)
            return null;

        await using var stream = await result.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public async Task ShareTextFileAsync(string fileName, string content, string title)
    {
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = title,
            File = new ShareFile(filePath)
        });
    }
}
