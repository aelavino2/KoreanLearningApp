using KoreanLearningApp.Navigation;
using KoreanLearningApp.Views;

namespace KoreanLearningApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(AppRoutes.WordDetails, typeof(WordsPage));
        Routing.RegisterRoute(AppRoutes.WordDetail, typeof(WordDetailPage));
    }
}