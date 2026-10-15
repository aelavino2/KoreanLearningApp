using CommunityToolkit.Mvvm.ComponentModel;

namespace KoreanLearningApp.ViewModels;

public partial class TopikLevelOptionViewModel(string code) : ObservableObject
{
    public string Code { get; } = code;

    public string Title => $"TOPIK {Code}";

    [ObservableProperty]
    private bool _isSelected;
}
