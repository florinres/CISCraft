using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;
using MonacoViewModel = Ui.ViewModels.Pages.MonacoViewModel;

namespace Ui.Views.Pages;

public partial class MonacoPage : INavigableView<MonacoViewModel>
{
    public MonacoViewModel ViewModel { get; }

    public MonacoPage(MonacoViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
        ViewModel.SetWebView(WebView);
    }
}