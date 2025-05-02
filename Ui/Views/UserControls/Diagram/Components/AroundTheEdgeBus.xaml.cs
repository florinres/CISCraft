using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ui.Views.UserControls.Diagram;

public partial class AroundTheEdgeBus : UserControl
{
    public static readonly DependencyProperty WrapWidthProperty =
        DependencyProperty.Register(nameof(WrapWidth), typeof(double), typeof(AroundTheEdgeBus), new PropertyMetadata(300.0));

    public static readonly DependencyProperty WrapHeightProperty =
        DependencyProperty.Register(nameof(WrapHeight), typeof(double), typeof(AroundTheEdgeBus), new PropertyMetadata(300.0));
    
    public static readonly DependencyProperty LineThicknessProperty =
        DependencyProperty.Register(nameof(LineThickness), typeof(double), typeof(AroundTheEdgeBus), new PropertyMetadata(6.0));

    public double WrapWidth 
    {
        get => (double)GetValue(WrapWidthProperty);
        set => SetValue(WrapWidthProperty, value);
    }

    public double WrapHeight 
    {
        get => (double)GetValue(WrapHeightProperty);
        set => SetValue(WrapHeightProperty, value);
    }
    
    public double LineThickness  
    {
        get => (double)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public Point BottomLeftpoint => new Point(0, WrapHeight);
    public Point BottomRightpoint => new Point(WrapWidth, WrapHeight);
    public Point TopRightpoint => new Point(WrapWidth, 0);

    public AroundTheEdgeBus()
    {
        InitializeComponent();

        // UpdateGeometry();
    }
    
    // private void UpdateGeometry()
    // {
    //     // Create a PathFigure with custom geometry
    //     var path = new PathFigure
    //     {
    //         StartPoint = new Point(0, 0),
    //         IsClosed = false
    //     };
    //
    //     path.Segments.Add(new LineSegment(new Point(0, WrapHeight), true));       // down
    //     path.Segments.Add(new LineSegment(new Point(WrapWidth, WrapHeight), true)); // right
    //     path.Segments.Add(new LineSegment(new Point(WrapWidth, 0), true));        // up
    //
    //     // Create the PathGeometry object
    //     var geometry = new PathGeometry();
    //     geometry.Figures.Add(path);
    //
    //     // Create a Path element to render the geometry
    //     var myPathElement = new Path
    //     {
    //         Data = geometry,
    //         Stroke = Brushes.Black,
    //         StrokeThickness = LineThickness
    //     };
    //
    //     // Add the path element to the visual tree (or a container in XAML)
    //     Content = myPathElement; // Replace with appropriate layout container
    // }
}