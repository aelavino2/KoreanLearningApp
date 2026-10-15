namespace KoreanLearningApp.Presentation.Navigation;

public interface INavigationService
{
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoToRootAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();

    /// <summary>
    /// Открывает страницу деталей, передавая параметр под ключом <see cref="NavigationParameters.Word"/>.
    /// </summary>
    Task GoToDetailAsync(string route, object parameter);
}
