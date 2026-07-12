using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class ImportExportPage : ContentPage
{
    private readonly ImportExportPageViewModel _viewModel;

    public ImportExportPage(DatabaseService db)
    {
        InitializeComponent();
        _viewModel = new ImportExportPageViewModel(db);
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshBackupPath();
    }
}