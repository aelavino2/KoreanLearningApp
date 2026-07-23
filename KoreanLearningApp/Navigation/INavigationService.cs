namespace KoreanLearningApp.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);
        Task GoToRootAsync(string route, IDictionary<string, object>? parameters = null);
        Task GoBackAsync();
        Task GoToDetailAsync(string route, object parameter);
    }
}