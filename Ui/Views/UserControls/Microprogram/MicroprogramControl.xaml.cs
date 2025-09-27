using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ui.Helpers;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Components.Microprogram;
using Ui.Views.Dialogs;

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
            ShowSearchLabelDialog();
        }
    }
    
    private void ShowSearchLabelDialog()
    {
        var searchDialog = new MicroprogramSearchDialog();
        
        if (searchDialog.ShowDialog() == true)
        {
            string searchText = searchDialog.SearchText;
            SearchForLabel(searchText);
        }
    }
    
    private void SearchMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowSearchLabelDialog();
    }
    
    private void SearchForLabel(string label)
    {
        if (ViewModel == null || string.IsNullOrWhiteSpace(label))
            return;
            
        // Use the viewmodel to search for the label
        int foundIndex = (ViewModel as MicroprogramViewModel)?.SearchForLabel(label) ?? -1;
        
        if (foundIndex >= 0)
        {
            // Temporarily set the current row to the found index to scroll to it and highlight it
            ViewModel.CurrentRow = foundIndex;
            
            // Make sure the found row is visible
            if (MicroprogramScrollViewer.Items.Count > foundIndex)
            {
                // First scroll to make the item visible
                MicroprogramScrollViewer.ScrollIntoView(MicroprogramScrollViewer.Items[foundIndex]);
                
                // Activate highlight through the IsCurrent property
                if (ViewModel.Rows.Count > foundIndex)
                {
                    ViewModel.Rows[foundIndex].IsCurrent = true;
                    ViewModel.Rows[foundIndex].HighlightOpacity = 1.0; // Ensure it starts fully visible
                }
                
                // Create a timer to gradually fade out the highlight
                var highlightTimer = new System.Windows.Threading.DispatcherTimer();
                highlightTimer.Interval = TimeSpan.FromMilliseconds(800); // Keep highlight visible for 0.8 seconds before starting fade
                
                highlightTimer.Tick += (s, e) =>
                {
                    highlightTimer.Stop();
                    
                    // Use a second timer to control the fade effect
                    var fadeTimer = new System.Windows.Threading.DispatcherTimer();
                    fadeTimer.Interval = TimeSpan.FromMilliseconds(30); // Update more frequently for smoother and faster fade
                    
                    double opacity = 1.0;
                    fadeTimer.Tick += (sender, args) =>
                    {
                        // Decrease opacity more quickly
                        opacity -= 0.10;
                        
                        if (opacity <= 0)
                        {
                            // When fully transparent, stop timer and remove highlight
                            fadeTimer.Stop();
                            ViewModel.Rows[foundIndex].IsCurrent = false;
                        }
                        else
                        {
                            // Create custom transparent version of the highlight color
                            // We'll need to update the XAML style to use this property
                            if (ViewModel.Rows[foundIndex] != null)
                            {
                                ViewModel.Rows[foundIndex].HighlightOpacity = opacity;
                            }
                        }
                    };
                    
                    fadeTimer.Start();
                };
                
                highlightTimer.Start();
            }
        }
        else
        {
            MessageBox.Show($"Label '{label}' not found.", "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    // Helper method is still used in other places
    
    // This method is preserved but renamed to avoid the duplicate definition
    private T RecursiveFindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T typedChild)
                return typedChild;
                
            // Recursively search child elements
            var result = RecursiveFindVisualChild<T>(child);
            if (result != null)
                return result;
        }
        
        return null;
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