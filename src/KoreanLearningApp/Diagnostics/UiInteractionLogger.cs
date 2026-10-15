using Microsoft.Extensions.Logging;

namespace KoreanLearningApp.Diagnostics;

/// <summary>
/// Логирует нажатия, переключатели и навигацию во всём окне, подписываясь на элементы по мере их появления в дереве.
/// </summary>
public sealed class UiInteractionLogger(ILogger<UiInteractionLogger> logger)
{
    public void Attach(Window window)
    {
        window.DescendantAdded += (_, e) => Subscribe(e.Element);
        window.DescendantRemoved += (_, e) => Unsubscribe(e.Element);

        if (window.Page is Shell shell)
            shell.Navigated += OnNavigated;
    }

    private void Subscribe(Element element)
    {
        switch (element)
        {
            case Button button:
                button.Clicked += OnButtonClicked;
                break;
            case CheckBox checkBox:
                checkBox.CheckedChanged += OnCheckBoxChanged;
                break;
            case Switch toggle:
                toggle.Toggled += OnSwitchToggled;
                break;
            case RadioButton radioButton:
                radioButton.CheckedChanged += OnRadioButtonChanged;
                break;
            case Picker picker:
                picker.SelectedIndexChanged += OnPickerChanged;
                break;
        }

        if (element is View view)
        {
            foreach (var tap in view.GestureRecognizers.OfType<TapGestureRecognizer>())
                tap.Tapped += OnTapped;
        }
    }

    private void Unsubscribe(Element element)
    {
        switch (element)
        {
            case Button button:
                button.Clicked -= OnButtonClicked;
                break;
            case CheckBox checkBox:
                checkBox.CheckedChanged -= OnCheckBoxChanged;
                break;
            case Switch toggle:
                toggle.Toggled -= OnSwitchToggled;
                break;
            case RadioButton radioButton:
                radioButton.CheckedChanged -= OnRadioButtonChanged;
                break;
            case Picker picker:
                picker.SelectedIndexChanged -= OnPickerChanged;
                break;
        }

        if (element is View view)
        {
            foreach (var tap in view.GestureRecognizers.OfType<TapGestureRecognizer>())
                tap.Tapped -= OnTapped;
        }
    }

    private void OnButtonClicked(object? sender, EventArgs e) =>
        LogInteraction("Click", (Button)sender!, ((Button)sender!).Text);

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is View view)
            LogInteraction("Tap", view, DescribeContent(view), e.Parameter);
    }

    private void OnCheckBoxChanged(object? sender, CheckedChangedEventArgs e) =>
        LogInteraction("Check", (CheckBox)sender!, e.Value.ToString());

    private void OnSwitchToggled(object? sender, ToggledEventArgs e) =>
        LogInteraction("Toggle", (Switch)sender!, e.Value.ToString());

    private void OnRadioButtonChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
            LogInteraction("Select", (RadioButton)sender!, ((RadioButton)sender!).Content?.ToString());
    }

    private void OnPickerChanged(object? sender, EventArgs e)
    {
        var picker = (Picker)sender!;
        LogInteraction("Pick", picker, picker.SelectedItem?.ToString());
    }

    private void OnNavigated(object? sender, ShellNavigatedEventArgs e) =>
        logger.LogInformation("Navigate {Source}: {From} -> {To}", e.Source, e.Previous?.Location, e.Current.Location);

    private void LogInteraction(string action, VisualElement element, string? content, object? parameter = null) =>
        logger.LogInformation("{Action} {Element} \"{Content}\" on {Page} (context: {Context}, parameter: {Parameter})",
            action,
            string.IsNullOrEmpty(element.StyleId) ? element.GetType().Name : $"{element.GetType().Name}#{element.StyleId}",
            content,
            FindPage(element)?.GetType().Name,
            element.BindingContext?.GetType().Name,
            parameter?.GetType().Name);

    private static string? DescribeContent(View view)
    {
        var descendants = view.GetVisualTreeDescendants();
        return descendants.OfType<Label>().Select(l => l.Text).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t))
               ?? descendants.OfType<Image>().Select(i => (i.Source as FileImageSource)?.File).FirstOrDefault(f => f is not null);
    }

    private static Page? FindPage(Element element)
    {
        var current = element.Parent;
        while (current is not null and not Page)
            current = current.Parent;

        return current as Page;
    }
}
