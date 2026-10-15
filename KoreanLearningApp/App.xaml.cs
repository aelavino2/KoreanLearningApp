using KoreanLearningApp.Diagnostics;

namespace KoreanLearningApp;

public partial class App : Application
{
    private readonly UiInteractionLogger _interactionLogger;

    public App(GlobalExceptionHandler exceptionHandler, UiInteractionLogger interactionLogger)
    {
        InitializeComponent();
        _interactionLogger = interactionLogger;
        exceptionHandler.Register();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        _interactionLogger.Attach(window);
        return window;
    }
}
