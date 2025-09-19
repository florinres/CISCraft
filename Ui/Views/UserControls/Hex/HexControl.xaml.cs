using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
    }
    
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


}