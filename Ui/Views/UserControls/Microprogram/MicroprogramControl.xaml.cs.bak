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
        
        // Add keyboard event handler for Ctrl+F
        PreviewKeyDown += MicroprogramControl_PreviewKeyDown;
        
        // Add loaded event handler for showing the shortcut tooltip
        Loaded += MicroprogramControl_Loaded;
    }
    
    private void MicroprogramControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Show the shortcut overlay temporarily to educate users about the Ctrl+F shortcut
        var fadeIn = FindResource("FadeInStoryboard") as Storyboard;
        fadeIn?.Begin();
        
        // After 3 seconds, fade out the overlay
        var fadeOut = FindResource("FadeOutStoryboard") as Storyboard;
        if (fadeOut != null)
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, args) =>
            {
                fadeOut.Begin();
                timer.Stop();
            };
            timer.Start();
        }
    }
    
    private void MicroprogramControl_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Check for Ctrl+F key combination
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
            // Temporarily set the current row to the found index to scroll to it
            ViewModel.CurrentRow = foundIndex;
            
            // Make sure the found row is visible
            if (MicroprogramScrollViewer.Items.Count > foundIndex)
            {
                MicroprogramScrollViewer.ScrollIntoView(MicroprogramScrollViewer.Items[foundIndex]);
                
                // Use dispatcher to ensure UI has updated before clearing the highlight
                Dispatcher.BeginInvoke(new Action(() => {
                    // Clear the highlight after scrolling
                    if (ViewModel.Rows.Count > foundIndex)
                    {
                        ViewModel.Rows[foundIndex].IsCurrent = false;
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
        else
        {
            MessageBox.Show($"Label '{label}' not found.", "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }
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