using Microsoft.Maui.Controls;

namespace KoreanLearningApp.Navigation
{
    public class ShellNavigationService : INavigationService
    {
        public Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            return parameters is null
                ? Shell.Current.GoToAsync(route)
                : Shell.Current.GoToAsync(route, parameters);
        }

        public Task GoToRootAsync(string route, IDictionary<string, object>? parameters = null)
        {
            var rootRoute = $"//{route}";
            return parameters is null
                ? Shell.Current.GoToAsync(rootRoute)
                : Shell.Current.GoToAsync(rootRoute, parameters);
        }

        public Task GoBackAsync()
        {
            return Shell.Current.GoToAsync("..");
        }

        public async Task GoToDetailAsync(string route, object parameter)
        {
            await Shell.Current.GoToAsync(route, new Dictionary<string, object>
            {
                ["Word"] = parameter
            });
        }
    }
}