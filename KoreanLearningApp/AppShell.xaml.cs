using KoreanLearningApp.Views;

namespace KoreanLearningApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(AddWordPage), typeof(AddWordPage));
        Routing.RegisterRoute(nameof(ImportExportPage), typeof(ImportExportPage));
    }
}
