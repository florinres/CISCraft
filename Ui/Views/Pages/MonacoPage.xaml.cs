using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Gallery.ViewModels.Windows;

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