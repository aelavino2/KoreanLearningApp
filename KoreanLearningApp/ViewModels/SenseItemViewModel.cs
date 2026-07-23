using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.ViewModels;

public partial class SenseItemViewModel : ObservableObject
{
    public Sense Sense { get; }

    public SenseItemViewModel(Sense sense)
    {
        Sense = sense;
    }

    public string EnText => Sense.En?.Word ?? string.Empty;
    public string EnDefinition => Sense.En?.Definition ?? string.Empty;
    public string RuText => Sense.Ru?.Word ?? string.Empty;
    public string RuDefinition => Sense.Ru?.Definition ?? string.Empty;

    [ObservableProperty]
    private bool _isExpanded;

    [RelayCommand]
    private void ToggleExpanded() => IsExpanded = !IsExpanded;
}