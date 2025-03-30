using Ui.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace Ui.Views.Pages;

public partial class DataPage : INavigableView<DataViewModel>
{
    public DataPage(DataViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public DataViewModel ViewModel { get; }
}