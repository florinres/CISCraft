using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using Ui.Interfaces.ViewModel;

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
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;

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
        if (e.NewValue is IHexViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        if (e.OldValue is IHexViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
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