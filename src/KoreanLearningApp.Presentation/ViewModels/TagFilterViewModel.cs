using CommunityToolkit.Mvvm.ComponentModel;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Presentation.Formatting;

namespace KoreanLearningApp.Presentation.ViewModels;

public partial class TagFilterViewModel(TagUsage usage) : ObservableObject
{
    public int TagId { get; } = usage.Tag.Id;

    public string Title { get; } = $"{usage.Tag.ToDisplayName()} ({usage.WordCount})";

    [ObservableProperty]
    private bool _isSelected;
}
