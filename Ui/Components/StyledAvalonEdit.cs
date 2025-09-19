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
    }

    private void OnThemeChanged(ApplicationTheme newTheme, Color color)
    {
        ApplyTheme();
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

    ~StyledAvalonEdit()
    {
        ApplicationThemeManager.Changed -= OnThemeChanged;
    }
}