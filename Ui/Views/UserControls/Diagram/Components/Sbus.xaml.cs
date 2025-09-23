using System.Windows.Controls;

namespace Ui.Views.UserControls.Diagram.Components;

public partial class Sbus : UserControl
{
    public static readonly DependencyProperty LineWidthProperty =
        DependencyProperty.Register(nameof(LineWidth), typeof(double), typeof(Sbus), new PropertyMetadata(300.0));

    public static readonly DependencyProperty LineHeightProperty =
        DependencyProperty.Register(nameof(LineHeight), typeof(double), typeof(Sbus), new PropertyMetadata(6.0));

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
    
    public Sbus()
    {
        InitializeComponent();
    }
    
    public (double Left, double Right) GetEdges(FrameworkElement relativeTo)
    {
        // Transform (0,0) = top-left corner of this control into coordinates of relativeTo
        var transform = ActualVisualElement.TransformToAncestor(relativeTo);
        var topLeft = transform.Transform(new Point(0, 0));

        // Right edge is top-left.X + ActualWidth
        double left = topLeft.X;
        double right = topLeft.X + ActualVisualElement.ActualWidth;

        return (left, right);
    }

}