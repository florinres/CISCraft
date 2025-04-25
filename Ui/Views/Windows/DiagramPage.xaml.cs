using System.Windows.Input;
using Ui.ViewModels.Components.Diagram;
using Wpf.Ui.Controls;

namespace Ui.Views.Windows;

public partial class DiagramPage : FluentWindow
{

    // private Dictionary<string, EventHandler> eventHandlers = new Dictionary<string, EventHandler>();
    public DiagramPage()
    {
        InitializeComponent();
        
        // R1Context.customEvent += eventHandlers[R1Context.Name];
        
        DataContext = new DiagramViewModel();
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            const double zoomFactor = 0.1;
            var transform = DiagramScaleTransform;

            if (e.Delta > 0)
            {
                transform.ScaleX += zoomFactor;
                transform.ScaleY += zoomFactor;
            }
            else
            {
                transform.ScaleX = Math.Max(0.1, transform.ScaleX - zoomFactor);
                transform.ScaleY = Math.Max(0.1, transform.ScaleY - zoomFactor);
            }

            e.Handled = true;
        }
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
        if (_isDragging)
        {
            Point currentPoint = e.GetPosition(DiagramScrollViewer);
            Vector delta = currentPoint - _scrollStartPoint;

            DiagramScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X - delta.X);
            DiagramScrollViewer.ScrollToVerticalOffset(_scrollStartOffset.Y - delta.Y);
        }
    }

    private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            DiagramScrollViewer.ReleaseMouseCapture();
            DiagramScrollViewer.Cursor = Cursors.Arrow;
        }
    }
    
    public void CenterOn(Point position)
    {
        DiagramScrollViewer.ScrollToHorizontalOffset(position.X - DiagramScrollViewer.ViewportWidth / 2);
        DiagramScrollViewer.ScrollToVerticalOffset(position.Y - DiagramScrollViewer.ViewportHeight / 2);
    }
}