using System.Windows.Controls;
using System.Windows.Media;

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
    
    /// <summary>
    /// Returns the connection points of this RegisterBlock relative to a given ancestor (e.g. the Canvas).
    /// </summary>
    public Models.RegisterBlockConnectionPoints GetConnectionPoints(Visual relativeTo)
    {
        var result = new Models.RegisterBlockConnectionPoints();

        // Get the absolute rectangle of this RegisterBlock relative to ancestor
        var transform = RegisterBlockBorder.TransformToVisual(relativeTo);
        Rect bounds = transform.TransformBounds(new Rect(new Point(0, 0), RenderSize));

        // Compute positions
        result.MidLeft = new Point(bounds.Left, bounds.Top + bounds.Height / 2);
        result.MidRight = new Point(bounds.Right, bounds.Top + bounds.Height / 2);
        result.RightPlusOffset = new Point(bounds.Right, bounds.Top + bounds.Height / 2 + 5);
        result.RightMinusOffset = new Point(bounds.Right, bounds.Top + bounds.Height / 2 - 5);
        result.LeftPlusOffset = new Point(bounds.Left, bounds.Top + bounds.Height / 2 + 5);
        result.LeftMinusOffset = new Point(bounds.Left, bounds.Top + bounds.Height / 2 - 5);
        result.TopOffsetMinus = new Point(bounds.Left + bounds.Width / 2 - 30, bounds.Top);
        result.TopOffsetPlus = new Point(bounds.Left + bounds.Width / 2 + 30, bounds.Top);
        result.MidBottom = new Point(bounds.Left + bounds.Width / 2, bounds.Bottom);
        
        return result;
    }
    
}