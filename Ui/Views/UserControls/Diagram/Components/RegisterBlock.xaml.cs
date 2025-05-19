using System.Windows.Controls;

namespace Ui.Views.UserControls.Diagram.Components;

public partial class RegisterBlock : UserControl
{
    public static readonly DependencyProperty WithBorderProperty =
        DependencyProperty.Register(nameof(WithBorder), typeof(bool), typeof(RegisterBlock),
            new PropertyMetadata(true));
    
    public static readonly DependencyProperty MinNameWidthProperty =
        DependencyProperty.Register(nameof(MinNameWidth), typeof(double), typeof(RegisterBlock),
            new PropertyMetadata(0d));

    public double MinNameWidth
    {
        get => (double)GetValue(WithBorderProperty);
        set => SetValue(WithBorderProperty, value);
    }

    public bool WithBorder
    {
        get => (bool)GetValue(WithBorderProperty);
        set => SetValue(WithBorderProperty, value);
    }

    public RegisterBlock()
    {
        InitializeComponent();
        // DataContext = this;
    }
    
}