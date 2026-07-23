using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class ImportExportPage : ContentPage
{
    public ImportExportPage(ImportExportViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}