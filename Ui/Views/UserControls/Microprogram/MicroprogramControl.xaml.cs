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
        
        PreviewKeyDown += MicroprogramControl_PreviewKeyDown;
    }
    
    private void MicroprogramControl_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            ShowGoToRowDialog();
        }
    }

    private Point _scrollStartPoint;
    private Point _scrollStartOffset;
    private bool _isDragging;
    
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
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            ViewModel.ZoomFactor += e.Delta > 0 ? 1 : -1;
            if (ViewModel.ZoomFactor < 8) ViewModel.ZoomFactor = 8;
            if (ViewModel.ZoomFactor > 40) ViewModel.ZoomFactor = 40;
            e.Handled = true;
            return;
        }
        
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        {
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - e.Delta / 3.0);
            e.Handled = true;
            return;
        }
        
        e.Handled = false;
    }
    
    private void VerticalScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        VerticalScrollViewer.ScrollToVerticalOffset(VerticalScrollViewer.VerticalOffset - e.Delta / 3.0);
        e.Handled = true;
    }
    
    /// <summary>
    /// Shows the Go To Row dialog and navigates to the specified row
    /// </summary>
    private void ShowGoToRowDialog()
    {
        if (ViewModel == null)
            return;

        var dialog = new GoToRowDialog();
        
        var parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            dialog.Owner = parentWindow;
        }

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            int rowNumber = dialog.ParsedRow;

            if (NavigateToRow(rowNumber))
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {

                        foreach (var row in ViewModel.Rows)
                        {
                            row.IsGoToTarget = false;
                        }

                        if (rowNumber >= 0 && rowNumber < ViewModel.Rows.Count)
                        {
                            ViewModel.Rows[rowNumber].IsGoToTarget = true;
                            var item = MicroprogramScrollViewer.Items[rowNumber];
                            MicroprogramScrollViewer.ScrollIntoView(item);
                            MicroprogramScrollViewer.Focus();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error navigating to row: {ex.Message}", "Navigation Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
            else
            {
                MessageBox.Show($"Cannot navigate to row {rowNumber}. It is outside the valid range (0-{MicroprogramScrollViewer.Items.Count - 1}).",
                    "Invalid Row", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
        /// <summary>
    /// Validates if the row number is within valid range and navigates to it
    /// </summary>
    /// <param name="rowNumber">The row number to navigate to</param>
    /// <returns>True if navigation was successful, false otherwise</returns>
    private bool NavigateToRow(int rowNumber)
    {
        if (ViewModel == null || MicroprogramScrollViewer == null)
            return false;

        // Check if row number is within valid range
        if (rowNumber < 0 || rowNumber >= MicroprogramScrollViewer.Items.Count)
            return false;

        return true;
    }
}