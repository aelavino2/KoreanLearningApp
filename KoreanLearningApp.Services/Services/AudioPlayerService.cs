using KoreanLearningApp.Services.Abstractions.Services;
using Plugin.Maui.Audio;

namespace KoreanLearningApp.Services.Services;

public class AudioPlayerService : IAudioPlayerService
{
    private readonly IAudioManager _audioManager;
    private static readonly HttpClient _httpClient = new();

    private IAudioPlayer? _currentPlayer;

    public AudioPlayerService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public bool IsPlaying => _currentPlayer?.IsPlaying ?? false;

    public async Task PlayAsync(string? audioUrl, string? audioFileName, CancellationToken cancellationToken = default)
    {
        Stop();

        var stream = await ResolveStreamAsync(audioUrl, audioFileName, cancellationToken);
        if (stream is null)
            return;

        _currentPlayer = _audioManager.CreatePlayer(stream);
        _currentPlayer.Play();
    }

    public void Stop()
    {
        if (_currentPlayer is null)
            return;

        _currentPlayer.Stop();
        _currentPlayer.Dispose();
        _currentPlayer = null;
    }

    private static async Task<Stream?> ResolveStreamAsync(string? audioUrl, string? audioFileName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(audioFileName))
        {
            var projectRoot = DevPaths.FindProjectRoot();
            if (projectRoot is not null)
            {
                var relative = audioFileName.Replace('/', Path.DirectorySeparatorChar)
                                             .Replace('\\', Path.DirectorySeparatorChar);
                var devPath = Path.Combine(projectRoot, "Resources", relative);

                if (File.Exists(devPath))
                    return File.OpenRead(devPath);
            }
        }
        
        if (!string.IsNullOrWhiteSpace(audioFileName))
        {
            try
            {
                return await FileSystem.OpenAppPackageFileAsync(audioFileName);
            }
            catch (IOException)
            {
            }
        }
        
        if (!string.IsNullOrWhiteSpace(audioUrl))
        {
            var bytes = await _httpClient.GetByteArrayAsync(audioUrl, cancellationToken);
            return new MemoryStream(bytes);
        }

        return null;
    }
}