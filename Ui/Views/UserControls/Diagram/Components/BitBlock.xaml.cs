using System.Windows.Controls;
using System.Windows.Media;

namespace Ui.Views.UserControls.Diagram.Components;

public partial class BitBlock : UserControl
{
    public BitBlock()
    {
        InitializeComponent();
    }

    public Point GetConnectionPointRelativeTo(Visual relativeTo)
    {
        // Get the absolute rectangle of this RegisterBlock relative to ancestor
        var transform = BitBlockBorder.TransformToVisual(relativeTo);
        Rect bounds = transform.TransformBounds(new Rect(new Point(0, 0), RenderSize));
        
        return new Point(bounds.Left + bounds.Width/4, bounds.Top);
    }
}