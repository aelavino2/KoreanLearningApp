namespace KoreanLearningApp.Models;

// Составляет корейские слоги из отдельных букв (жамо) по формуле Unicode Hangul.
// Позволяет реализовать собственную клавиатуру ввода хангыля без системной раскладки.
public static class HangulComposer
{
    public static readonly string[] Cho =
    {
        "ㄱ","ㄲ","ㄴ","ㄷ","ㄸ","ㄹ","ㅁ","ㅂ","ㅃ","ㅅ",
        "ㅆ","ㅇ","ㅈ","ㅉ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ"
    };

    public static readonly string[] Jung =
    {
        "ㅏ","ㅐ","ㅑ","ㅒ","ㅓ","ㅔ","ㅕ","ㅖ","ㅗ","ㅘ",
        "ㅙ","ㅚ","ㅛ","ㅜ","ㅝ","ㅞ","ㅟ","ㅠ","ㅡ","ㅢ","ㅣ"
    };

    // Индекс 0 = нет конечной согласной (받침)
    public static readonly string[] Jong =
    {
        "", "ㄱ","ㄲ","ㄳ","ㄴ","ㄵ","ㄶ","ㄷ","ㄹ","ㄺ","ㄻ",
        "ㄼ","ㄽ","ㄾ","ㄿ","ㅀ","ㅁ","ㅂ","ㅄ","ㅅ","ㅆ",
        "ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ"
    };

    // Подмножество для клавиатуры финальных согласных — самые частые
    public static readonly string[] CommonJong =
    {
        "ㄱ","ㄴ","ㄷ","ㄹ","ㅁ","ㅂ","ㅅ","ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ","ㄲ","ㅆ"
    };

    public static char? Compose(int choIdx, int jungIdx, int jongIdx)
    {
        if (choIdx < 0 || choIdx >= Cho.Length) return null;
        if (jungIdx < 0 || jungIdx >= Jung.Length) return null;
        if (jongIdx < 0 || jongIdx >= Jong.Length) jongIdx = 0;

        int code = 0xAC00 + (choIdx * 21 + jungIdx) * 28 + jongIdx;
        return (char)code;
    }
}