
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ui.Views.UserControls.Diagram.Components;

/// <summary>
/// Represents a connector control that can be highlighted and customized with various properties such as stroke, thickness, and points.
/// </summary>
public partial class HighlightableConnector : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HighlightableConnector"/> class.
    /// </summary>
    public HighlightableConnector()
    {
        InitializeComponent();
        UpdatePath();
    }

    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(HighlightableConnector),
            new PropertyMetadata(new PointCollection(), (d, e) => ((HighlightableConnector)d).UpdatePath()));

    public PointCollection Points
    {
        get => (PointCollection)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(HighlightableConnector), new PropertyMetadata(Brushes.Black));

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(HighlightableConnector), new PropertyMetadata(2.0));

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty HighlightStrokeProperty =
        DependencyProperty.Register(nameof(HighlightStroke), typeof(Brush), typeof(HighlightableConnector), new PropertyMetadata(Brushes.Red));

    public Brush HighlightStroke
    {
        get => (Brush)GetValue(HighlightStrokeProperty);
        set => SetValue(HighlightStrokeProperty, value);
    }

    public static readonly DependencyProperty HighlightThicknessProperty =
        DependencyProperty.Register(nameof(HighlightThickness), typeof(double), typeof(HighlightableConnector), new PropertyMetadata(4.0));

    public double HighlightThickness
    {
        get => (double)GetValue(HighlightThicknessProperty);
        set => SetValue(HighlightThicknessProperty, value);
    }

    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register(nameof(IsHighlighted), typeof(bool), typeof(HighlightableConnector), new PropertyMetadata(false, (d, e) => ((HighlightableConnector)d).UpdatePath()));

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }
    
    public static readonly DependencyProperty TagProperty =
        DependencyProperty.Register(nameof(Tag), typeof(string), typeof(HighlightableConnector), new PropertyMetadata(string.Empty));

    public string Tag
    {
        get => (string)GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }

    private void UpdatePath()
    {
        if (Points == null || Points.Count < 2) return;

        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = Points[0], IsClosed = false };

        // add line segments dynamically for all remaining points
        for (int i = 1; i < Points.Count; i++)
        {
            figure.Segments.Add(new LineSegment(Points[i], true));
        }

        geometry.Figures.Add(figure);
        LinePath.Data = geometry;
    }

}