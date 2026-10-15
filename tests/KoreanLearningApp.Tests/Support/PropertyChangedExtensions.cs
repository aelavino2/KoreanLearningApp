using System.ComponentModel;

namespace KoreanLearningApp.Tests.Support;

public static class PropertyChangedExtensions
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Ждёт, пока условие станет истинным после очередного PropertyChanged; нужно для логики,
    /// которая продолжается асинхронно после срабатывания таймера.
    /// </summary>
    public static async Task WaitUntilAsync(this INotifyPropertyChanged source, Func<bool> condition)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        PropertyChangedEventHandler handler = (_, _) =>
        {
            if (condition())
                completion.TrySetResult();
        };

        source.PropertyChanged += handler;
        try
        {
            if (condition())
                completion.TrySetResult();

            await completion.Task.WaitAsync(Timeout);
        }
        finally
        {
            source.PropertyChanged -= handler;
        }
    }
}
