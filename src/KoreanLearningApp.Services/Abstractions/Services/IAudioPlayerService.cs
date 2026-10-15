namespace KoreanLearningApp.Services.Abstractions.Services
{
    public interface IAudioPlayerService
    {
        Task PlayAsync(string? audioUrl, string? audioFileName, CancellationToken cancellationToken = default);

        void Stop();

        bool IsPlaying { get; }
    }
}
