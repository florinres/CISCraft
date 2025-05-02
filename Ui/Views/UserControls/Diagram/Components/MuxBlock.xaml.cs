using System.Windows.Controls;
using System.Windows.Media;

namespace Ui.Views.UserControls.Diagram;

public partial class MuxBlock : UserControl
{
    public MuxBlock()
    {
        InitializeComponent();
    }
    
    public bool IsFlipped
    {
        get => (bool)GetValue(IsFlippedProperty);
        set => SetValue(IsFlippedProperty, value);
    }

    public static readonly DependencyProperty IsFlippedProperty =
        DependencyProperty.Register(nameof(IsFlipped), typeof(bool), typeof(MuxBlock),
            new PropertyMetadata(false, OnIsFlippedChanged));

    private static void OnIsFlippedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MuxBlock mux)
        {
            bool flipped = (bool)e.NewValue;
            mux.ShapeLayer.LayoutTransform = flipped ? new ScaleTransform(-1, 1) : Transform.Identity;
        }
    }

}