using System.Windows.Controls;
using Ui.Interfaces.ViewModel;

namespace Ui.Views.UserControls.Hex;

public partial class HexControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(IHexViewModel), typeof(HexControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public IHexViewModel ViewModel
    {
        get => (IHexViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HexControl control && e.NewValue is IHexViewModel vm)
        {
            control.DataContext = vm;
        }
    }
    
    public HexControl()
    {
        InitializeComponent();
    }
    
    
}