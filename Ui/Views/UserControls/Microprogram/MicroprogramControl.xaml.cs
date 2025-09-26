using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ui.Helpers;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Components.Microprogram;

namespace Ui.Views.UserControls.Microprogram;

public partial class MicroprogramControl : UserControl
{
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
                control.FontSize = vm.ZoomFactor;
                if (args.PropertyName != nameof(vm.CurrentRow)) return;
                
                var index = vm.CurrentRow;
                if (index < 0 || index >= control.MicroprogramScrollViewer.Items.Count) return;
                
                var item = control.MicroprogramScrollViewer.Items[index];
                
                if(item is not null)
                {
                    control.MicroprogramScrollViewer.ScrollIntoView(item);
                    
                    // Use dispatcher to ensure UI has updated before attempting to scroll
                    control.Dispatcher.BeginInvoke(new Action(() => {
                        control.MicroprogramScrollViewer.UpdateLayout();
                        control.MicroprogramScrollViewer.ScrollIntoView(item);
                    }));
                }
            };
        }
    }
    
    public MicroprogramControl()
    {
        InitializeComponent();
    }

    private Point _scrollStartPoint;
    private Point _scrollStartOffset;
    private bool _isDragging;
    
    // No longer needed as we're directly working with MainScrollViewer
    
    private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _scrollStartPoint = e.GetPosition(MainScrollViewer);
        _scrollStartOffset.X = MainScrollViewer.HorizontalOffset;
        _scrollStartOffset.Y = MainScrollViewer.VerticalOffset;
        _isDragging = true;
        MainScrollViewer.CaptureMouse();
        MainScrollViewer.Cursor = Cursors.Hand;
    }

    private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        
        Point currentPoint = e.GetPosition(MainScrollViewer);
        Vector delta = currentPoint - _scrollStartPoint;

        // Invert the direction by adding delta.X instead of subtracting it
        // This makes dragging right move the content right (natural scrolling)
        MainScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X + delta.X);
    }

    private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        MainScrollViewer.ReleaseMouseCapture();
        MainScrollViewer.Cursor = Cursors.Arrow;
    }

    private void HorizontalScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // If Ctrl is pressed, handle zooming
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            ViewModel.ZoomFactor += e.Delta > 0 ? 1 : -1;
            if (ViewModel.ZoomFactor < 8) ViewModel.ZoomFactor = 8;       // Min font size
            if (ViewModel.ZoomFactor > 40) ViewModel.ZoomFactor = 40;     // Max font size
            e.Handled = true;
            return;
        }
        
        // If Shift is pressed, scroll horizontally
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        {
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - e.Delta / 3.0);
            e.Handled = true;
            return;
        }
        
        // Pass the event to the vertical scrollviewer (let it bubble to the VerticalScroll_PreviewMouseWheel)
        e.Handled = false;
    }
    
    private void VerticalScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Handle vertical scrolling
        VerticalScrollViewer.ScrollToVerticalOffset(VerticalScrollViewer.VerticalOffset - e.Delta / 3.0);
        e.Handled = true;
    }
}