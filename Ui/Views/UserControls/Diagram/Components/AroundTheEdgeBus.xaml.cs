using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ui.Models;

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

    public double GetEdgeAbsolute(PathSide side, EdgeType edgeType, FrameworkElement container)
    {
        var path = AroundTheEdgePath;
        if (path.Data is not PathGeometry pg ||
            pg.Figures.Count == 0 ||
            pg.Figures[0].Segments.Count < 2)
            throw new InvalidOperationException("Unexpected Path data structure.");

        var fig = pg.Figures[0];
        var bl = fig.Segments[0] is LineSegment ls0 ? ls0.Point : default; // BottomLeft
        var br = fig.Segments[1] is LineSegment ls1 ? ls1.Point : default; // BottomRight
        var tr = fig.Segments[2] is LineSegment ls2 ? ls2.Point : default; // TopRight
        var tl = fig.StartPoint; // TopLeft

        double halfThickness = path.StrokeThickness / 2.0;

        Point targetPoint;

        switch (side)
        {
            case PathSide.Left:
                targetPoint = tl; // take top-left, vertical line
                break;
            case PathSide.Right:
                targetPoint = tr; // take top-right, vertical line
                break;
            case PathSide.Bottom:
                targetPoint = bl; // take bottom-left, horizontal line
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
        }

        // Compute direction and normal for vertical edges
        Vector normal;
        if (side == PathSide.Left || side == PathSide.Right)
        {
            Point start = side == PathSide.Left ? tl : tr;
            Point end = side == PathSide.Left ? bl : br;
            Vector dir = end - start;
            dir.Normalize();

            if (side == PathSide.Right)
                normal = new Vector(-dir.Y, dir.X) * halfThickness * (edgeType == EdgeType.Inner ? 1 : -1);
            else // Right edge
                normal = new Vector(-dir.Y, dir.X) * halfThickness * (edgeType == EdgeType.Inner ? -1 : 1);

            targetPoint += normal;
            targetPoint = path.TransformToAncestor(container).Transform(targetPoint);
            return targetPoint.X;
        }

        else // bottom edge, horizontal, only inner Y
        {
            Vector dir = br - bl;
            dir.Normalize();
            normal = new(-dir.Y, dir.X);

            // Inner edge only
            normal *= -halfThickness;
            targetPoint += normal;

            targetPoint = path.TransformToAncestor(container).Transform(targetPoint);

            // Return absolute Y
            return targetPoint.Y;
        }
    }
}