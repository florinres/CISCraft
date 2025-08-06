using System.Windows.Input;
using System.Windows.Interop;
using AvalonDock.Layout;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.MenuBar;
using Wpf.Ui.Controls;

namespace Ui.Views.UserControls.MenuBar;

public partial class LayoutControl : MenuItem
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(ILayoutControlViewModel), typeof(LayoutControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public ILayoutControlViewModel ViewModel
    {
        get => (ILayoutControlViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LayoutControl control && e.NewValue is ILayoutControlViewModel vm)
        {
            control.DataContext = vm;
        }
    }
    public LayoutControl()
    {
        InitializeComponent();
        if (ViewModel != null)
            DataContext = ViewModel;
        // Loaded += OnLoaded;
        // DataContextChanged += OnDataContextChanged;
    }
    
    

}