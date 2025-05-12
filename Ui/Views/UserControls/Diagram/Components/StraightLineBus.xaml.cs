using System.Windows.Controls;

namespace Ui.Views.UserControls.Diagram;

public partial class StraightLineBus : UserControl
{
    public static readonly DependencyProperty LineWidthProperty =
        DependencyProperty.Register(nameof(LineWidth), typeof(double), typeof(StraightLineBus), new PropertyMetadata(300.0));

    public static readonly DependencyProperty LineHeightProperty =
        DependencyProperty.Register(nameof(LineHeight), typeof(double), typeof(StraightLineBus), new PropertyMetadata(6.0));

    public double LineWidth
    {
        get => (double)GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }
    
    public StraightLineBus()
    {
        InitializeComponent();
    }
}