using System.Windows.Controls;
using System.Windows.Input;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Diagram;

namespace Ui.Views.UserControls.Diagram;

public partial class DiagramUserControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(IDiagramViewModel), typeof(DiagramUserControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public IDiagramViewModel ViewModel
    {
        get => (IDiagramViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DiagramUserControl control && e.NewValue is IDiagramViewModel vm)
        {
            control.DataContext = vm;
        }
    }
    
    public DiagramUserControl()
    {
        InitializeComponent();
    }
    
    
    
    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

        const double zoomStep = 0.1;
        var newZoom = ViewModel.ZoomFactor;

        if (e.Delta > 0)
        {
            newZoom += zoomStep;
        }
        else
        {
            newZoom = Math.Max(0.1, newZoom - zoomStep);
        }

        ViewModel.ZoomFactor = newZoom;
        e.Handled = true;

    }

    private Point _scrollStartPoint;
    private Point _scrollStartOffset;
    private bool _isDragging;

    private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _scrollStartPoint = e.GetPosition(DiagramScrollViewer);
        _scrollStartOffset.X = DiagramScrollViewer.HorizontalOffset;
        _scrollStartOffset.Y = DiagramScrollViewer.VerticalOffset;
        _isDragging = true;
        DiagramScrollViewer.CaptureMouse();
        DiagramScrollViewer.Cursor = Cursors.Hand;
    }

    private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        
        Point currentPoint = e.GetPosition(DiagramScrollViewer);
        Vector delta = currentPoint - _scrollStartPoint;

        DiagramScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X - delta.X);
        DiagramScrollViewer.ScrollToVerticalOffset(_scrollStartOffset.Y - delta.Y);
    }

    private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        DiagramScrollViewer.ReleaseMouseCapture();
        DiagramScrollViewer.Cursor = Cursors.Arrow;
    }
    
    public void CenterOn(Point position)
    {
        DiagramScrollViewer.ScrollToHorizontalOffset(position.X - DiagramScrollViewer.ViewportWidth / 2);
        DiagramScrollViewer.ScrollToVerticalOffset(position.Y - DiagramScrollViewer.ViewportHeight / 2);
    }
}