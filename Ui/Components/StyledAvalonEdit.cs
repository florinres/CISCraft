using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Ui.Helpers;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using System.Windows.Input;

namespace Ui.Components;

public class StyledAvalonEdit : TextEditor, IAppearanceControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StyledAvalonEdit),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    // Add a RoutedCommand for Save
    public static readonly RoutedCommand SaveCommand = new RoutedCommand(
        "Save", 
        typeof(StyledAvalonEdit),
        new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) });
    
    // Add constants for min/max font size
    private const double MinFontSize = 8.0;
    private const double MaxFontSize = 36.0;
    private const double DefaultFontSize = 10.0;
    
    public StyledAvalonEdit()
    {
        //theming
        Loaded += OnLoaded;
        ApplicationThemeManager.Changed += OnThemeChanged;

        //textChanged
        TextChanged += (s, e) =>
        {
            if (Document == null) return;

            var text = Document.Text;
            if (Text != text) Text = text;
        };

        //syntaxHighlighting
        SyntaxHighlighting = AvalonEditHelper.AssemblyHighlightDefinition;
        
        // Add command bindings
        CommandBindings.Add(new CommandBinding(SaveCommand, ExecuteSaveCommand));
        
        // Add zoom support with mouse wheel
        PreviewMouseWheel += OnPreviewMouseWheel;
    }
    
    private void ExecuteSaveCommand(object sender, ExecutedRoutedEventArgs e)
    {
        // Raise a save requested event that MainWindow can listen for
        SaveRequested?.Invoke(this, EventArgs.Empty);
    }
    
    // Event that will be raised when Ctrl+S is pressed
    public event EventHandler? SaveRequested;

    public new string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ControlAppearance Appearance { get; set; }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (StyledAvalonEdit)d;

        // Prevent unnecessary setting if content is already equal
        if (editor.Document.Text != (string)e.NewValue) editor.Document.Text = (string)e.NewValue ?? "";
    }


    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyTheme();
        UpdateSyntaxHighlighting();
    }

    private void OnThemeChanged(ApplicationTheme newTheme, Color color)
    {
        ApplyTheme();
        UpdateSyntaxHighlighting();
    }

    private void ApplyTheme()
    {
        var bg = (Brush)FindResource("ApplicationBackgroundBrush");
        var fg = (Brush)FindResource("TextFillColorPrimaryBrush");

        TextArea.Background = bg;
        TextArea.Foreground = fg;

        Background = bg; // WPF background of the control
        TextArea.Background = bg;
        Foreground = fg; // WPF foreground of the control
        TextArea.Foreground = fg;
    }
    
    private void UpdateSyntaxHighlighting()
    {
        try
        {
            // Create a new instance of the highlighting definition every time
            var highlightDefinition = AvalonEditHelper.AssemblyHighlightDefinition;
            
            // Reset and then set the highlighting to ensure it refreshes
            SyntaxHighlighting = null;
            SyntaxHighlighting = highlightDefinition;
            
            // If this is a ThemeAwareHighlightingDefinition, subscribe to its ThemeChanged event
            if (SyntaxHighlighting is ThemeAwareHighlightingDefinition themeAwareDefinition)
            {
                // Debug output to confirm we're setting up theme change handling
                System.Diagnostics.Debug.WriteLine("Setting up theme change handler on ThemeAwareHighlightingDefinition");
                
                // Unsubscribe first to avoid multiple handlers
                themeAwareDefinition.ThemeChanged -= OnHighlightingThemeChanged;
                themeAwareDefinition.ThemeChanged += OnHighlightingThemeChanged;
            }
            
            System.Diagnostics.Debug.WriteLine("Updated syntax highlighting based on theme change");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating syntax highlighting: {ex.Message}");
        }
    }
    
    private void OnHighlightingThemeChanged(object? sender, EventArgs e)
    {
        // Force the editor to redraw with the new highlighting colors
        System.Diagnostics.Debug.WriteLine("Highlighting theme changed, forcing redraw");
        
        // Refresh the TextArea to update the colors
        TextArea.TextView.Redraw();
    }
    
    // Handle zoom functionality using Ctrl+MouseWheel
    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            double fontSize = FontSize;
            
            if (e.Delta > 0)
            {
                // Zoom in
                fontSize = Math.Min(fontSize + 1, MaxFontSize);
            }
            else
            {
                // Zoom out
                fontSize = Math.Max(fontSize - 1, MinFontSize);
            }
            
            if (fontSize != FontSize)
            {
                FontSize = fontSize;
                System.Diagnostics.Debug.WriteLine($"Font size changed to: {FontSize}pt");
            }
            
            e.Handled = true; // Prevent scrolling
        }
    }
    
    // Add a method to reset zoom to default
    public void ResetZoom()
    {
        FontSize = DefaultFontSize;
    }

    ~StyledAvalonEdit()
    {
        ApplicationThemeManager.Changed -= OnThemeChanged;
        PreviewMouseWheel -= OnPreviewMouseWheel;
    }
}