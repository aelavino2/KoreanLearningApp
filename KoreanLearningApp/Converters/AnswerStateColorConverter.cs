using System.Globalization;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Converters;

/// <summary>
/// Превращает AnswerOptionState варианта ответа квиза в цвет.
/// ConverterParameter задаёт роль цвета: "Background", "Stroke" или "Text".
/// Для состояния Default подставляет цвета приложения (если заданы в ресурсах),
/// иначе — нейтральный серый по умолчанию, чтобы конвертер работал даже без
/// подключённой темы приложения.
/// </summary>
public class AnswerStateColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var state = value is AnswerOptionState s ? s : AnswerOptionState.Default;
        var role = parameter as string ?? "Background";

        return (state, role) switch
        {
            (AnswerOptionState.Correct, "Background") => Color.FromArgb("#DCFCE7"),
            (AnswerOptionState.Correct, "Stroke") => Color.FromArgb("#22C55E"),
            (AnswerOptionState.Correct, "Text") => Color.FromArgb("#15803D"),

            (AnswerOptionState.Incorrect, "Background") => Color.FromArgb("#FEE2E2"),
            (AnswerOptionState.Incorrect, "Stroke") => Color.FromArgb("#EF4444"),
            (AnswerOptionState.Incorrect, "Text") => Color.FromArgb("#B91C1C"),

            (_, "Background") => ResourceOrDefault("ColorChipBg", Color.FromArgb("#F3F4F6")),
            (_, "Stroke") => Color.FromArgb("#E5E7EB"),
            (_, "Text") => ResourceOrDefault("ColorPrimaryDark", Color.FromArgb("#1F2937")),
            _ => Colors.Transparent
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static Color ResourceOrDefault(string key, Color fallback) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : fallback;
}
