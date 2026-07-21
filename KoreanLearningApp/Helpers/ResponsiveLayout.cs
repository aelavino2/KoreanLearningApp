namespace KoreanLearningApp.Helpers;

public static class ResponsiveLayout
{
    public static int GetColumnSpan(double width) => width switch
    {
        < 600 => 1,
        < 900 => 2,
        _ => 3
    };
}