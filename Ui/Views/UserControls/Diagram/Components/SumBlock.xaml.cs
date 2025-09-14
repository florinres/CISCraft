using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ui.Views.UserControls.Diagram;

public partial class SumBlock : UserControl
{
    public SumBlock()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Returns connection points of this SumBlock relative to a given ancestor (e.g., MainDiagramGrid or ConnectionCanvas).
    /// Uses the control's rendered bounds (the Viewbox+rotation are already baked into RenderSize/layout).
    /// </summary>
    public Ui.Models.RegisterBlockConnectionPoints GetConnectionPoints(Visual relativeTo)
    {
        var t = TransformToVisual(relativeTo);
        Rect bounds = t.TransformBounds(new Rect(new Point(0, 0), RenderSize));

        double midX = bounds.Left + bounds.Width / 2.0;
        double midY = bounds.Top  + bounds.Height / 2.0;

        return new Ui.Models.RegisterBlockConnectionPoints
        {
            MidLeft         = new Point(bounds.Left,  midY),
            MidRight        = new Point(bounds.Right, midY),
            LeftPlusOffset  = new Point(bounds.Left,  midY + 5),
            LeftMinusOffset = new Point(bounds.Left,  midY - 5),
            RightPlusOffset = new Point(bounds.Right, midY + 5),
            RightMinusOffset= new Point(bounds.Right, midY - 5),
            TopOffsetMinus  = new Point(midX - 30, bounds.Top),
            TopOffsetPlus   = new Point(midX + 30, bounds.Top),
            MidBottom       = new Point(midX, bounds.Bottom)
        };
    }
}
