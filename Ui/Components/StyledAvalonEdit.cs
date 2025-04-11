using System.ComponentModel;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Ui.Components
{
    public class StyledAvalonEdit : TextEditor, IAppearanceControl
    {
        public ControlAppearance Appearance { get; set; }

        public StyledAvalonEdit()
        {
            Loaded += OnLoaded;
            ApplicationThemeManager.Changed += OnThemeChanged;
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

            Background = bg;  // WPF background of the control
            TextArea.Background = bg;
            Foreground = fg;  // WPF foreground of the control
            TextArea.Foreground = fg;
        }
        
        ~StyledAvalonEdit()
        {
            ApplicationThemeManager.Changed -= OnThemeChanged;
        }
    }
}
