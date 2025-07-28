using System.Windows.Controls;
using System.Windows.Input;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Components.Microprogram;

namespace Ui.Views.UserControls.Microprogram;

public partial class MicroprogramControl : UserControl
{
    
    public new static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(
            "FontSize", typeof(double), typeof(MicroprogramControl),
            new PropertyMetadata(12.0));

    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
    
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(IMicroprogramViewModel), typeof(MicroprogramControl),
            new PropertyMetadata(null, OnViewModelChanged));


    public IMicroprogramViewModel ViewModel
    {
        get => (IMicroprogramViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }


    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MicroprogramControl control && e.NewValue is IMicroprogramViewModel vm)
        {
            control.DataContext = vm;

            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(vm.CurrentRow)) return;
                
                var index = vm.CurrentRow;
                if (index < 0 || index >= control.MicroprogramScrollViewer.Items.Count) return;
                
                var item = control.MicroprogramScrollViewer.Items[index];
                
                if(item is not null)
                    control.MicroprogramScrollViewer.ScrollIntoView(item);
            };
        }
    }
    
    public MicroprogramControl()
    {
        InitializeComponent();
    }

    private Point _scrollStartPoint;
    private bool _isDragging;
    private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _scrollStartPoint = e.GetPosition(MicroprogramScrollViewer);
        // _scrollStartOffset.X = MicroprogramScrollViewer.HorizontalOffset;
        // _scrollStartOffset.Y = MicroprogramScrollViewer.VerticalOffset;
        _isDragging = true;
        MicroprogramScrollViewer.CaptureMouse();
        MicroprogramScrollViewer.Cursor = Cursors.Hand;
    }

    private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        
        Point currentPoint = e.GetPosition(MicroprogramScrollViewer);
        Vector delta = currentPoint - _scrollStartPoint;

        // MicroprogramScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X - delta.X);
        // MicroprogramScrollViewer.ScrollToVerticalOffset(_scrollStartOffset.Y - delta.Y);
    }

    private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        MicroprogramScrollViewer.ReleaseMouseCapture();
        MicroprogramScrollViewer.Cursor = Cursors.Arrow;
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = false;
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;
        
        FontSize += e.Delta > 0 ? 1 : -1;
        if (FontSize < 8) FontSize = 8;       // Min font size
        if (FontSize > 40) FontSize = 40;     // Max font size
        e.Handled = true;
    }

    
}