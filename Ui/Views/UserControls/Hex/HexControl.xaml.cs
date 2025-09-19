using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Ui.Interfaces.ViewModel;
using WpfHexaEditor;

namespace Ui.Views.UserControls.Hex;

public partial class HexControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(IHexViewModel), typeof(HexControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public IHexViewModel ViewModel
    {
        get => (IHexViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HexControl control && e.NewValue is IHexViewModel vm)
        {
            control.DataContext = vm;
        }
    }
    
    public HexControl()
    {
        InitializeComponent();

        // Enable double-buffering to prevent flickering
        EnableDoubleBuffering();

        // Set the rendering options to optimize performance
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        RenderOptions.SetCachingHint(this, CachingHint.Cache);
        
        // Disable animations that might cause flickering
        UseLayoutRounding = true;
        
        // Set the rendering tier to force hardware acceleration if possible
        var renderingTier = RenderCapability.Tier >> 16;
        if (renderingTier >= 2) // Tier 2 or higher supports hardware acceleration
        {
            // Hardware acceleration is enabled by default in WPF Tier 2.
            // No need to set RenderMode, as CompositionTarget does not expose this property.
            // You may optionally log or handle tier-specific logic here.
        }
        
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
        
        // Add keyboard event handler for Ctrl+F
        PreviewKeyDown += HexControl_PreviewKeyDown;
    }
    
    private void HexControl_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Check for Ctrl+F key combination
        if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            ShowGoToAddressDialog();
        }
    }

    // Enable WPF double-buffering to prevent flickering
    private void EnableDoubleBuffering()
    {
        if (PresentationSource.FromVisual(this) != null)
        {
            var hwndTarget = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndTarget != null)
            {
                var hwndTargetComposition = hwndTarget.CompositionTarget;
                if (hwndTargetComposition != null)
                {
                    hwndTargetComposition.RenderMode = RenderMode.SoftwareOnly;
                }
            }
        }
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PatchStatusBarGridBackground(HexEditorControl);
        
        // Show the shortcut overlay temporarily to educate users about the Ctrl+F shortcut
        //ShowShortcutOverlay();
    }
    
    //private void ShowShortcutOverlay()
    //{
    //    // Only show the overlay if it hasn't been shown before in this session
    //    if (ShortcutOverlay.Visibility == Visibility.Collapsed)
    //    {
    //        ShortcutOverlay.Visibility = Visibility.Visible;
            
    //        // Set up a timer to hide the overlay after 5 seconds
    //        var timer = new System.Windows.Threading.DispatcherTimer
    //        {
    //            Interval = TimeSpan.FromSeconds(5)
    //        };
    //        timer.Tick += (s, e) => 
    //        {
    //            ShortcutOverlay.Visibility = Visibility.Collapsed;
    //            timer.Stop();
    //        };
    //        timer.Start();
    //    }
    //}
    
    private void PatchStatusBarGridBackground(DependencyObject root)
    {
        var background = TryFindResource("ApplicationBackgroundBrush") as Brush;
        if (background is null) return;

        Grid? statusBarGrid = FindChildByName<Grid>(root, "StatusBarGrid");
        if (statusBarGrid is not null)
        {
            statusBarGrid.Background = background;
        }
    }

    private T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild && typedChild.Name == name)
                return typedChild;

            // Recurse
            T? result = FindChildByName<T>(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
    
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is IHexViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is IHexViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            if (vm.ZoomFactor < 0.5) vm.ZoomFactor = 0.5;
            if (vm.ZoomFactor > 2.0001) vm.ZoomFactor = 2;
            HexEditorControl.ZoomScale = vm.ZoomFactor;
            HexEditorControl.InvalidateVisual();
        }
    }
    
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IHexViewModel.HexEditorStream))
        {
            Dispatcher.Invoke(() =>
            {
                HexEditorControl.Stream = null;
                HexEditorControl.Stream = ((IHexViewModel)sender!).HexEditorStream;
            });
        }
    }
    
    /// <summary>
    /// Shows the Go To Address dialog and navigates to the entered address
    /// </summary>
    private void ShowGoToAddressDialog()
    {
        if (ViewModel == null)
            return;

        var dialog = new GoToAddressDialog();
        
        // Set the owner to the parent window to make it modal
        var parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            dialog.Owner = parentWindow;
        }
        
        // Show the dialog and handle the result
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            // User clicked OK, get the address and navigate to it
            long address = dialog.ParsedAddress;
            
            if (ViewModel.GotoAddress(address))
            {
                // If the ViewModel successfully handled the address, set the position in the HexEditor
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Set the position and move the view to center on the position
                        HexEditorControl.SetPosition(address, 1);
                        // HexEditorControl.ScrollToLine(HexEditorControl.GetLineFromPosition(address));
                        // Instead, use SetPosition to move the caret, and optionally call UpdateFocus to ensure visibility
                        HexEditorControl.UpdateFocus();
                        HexEditorControl.Focus();
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that might occur
                        MessageBox.Show($"Error navigating to address: {ex.Message}", "Navigation Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
            else
            {
                // The address was invalid or out of range
                MessageBox.Show($"Cannot navigate to address 0x{address:X}. It is outside the valid range.",
                    "Invalid Address", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}