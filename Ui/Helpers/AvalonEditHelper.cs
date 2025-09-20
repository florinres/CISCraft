using System.Xml;
using System.IO;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Wpf.Ui.Appearance;

namespace Ui.Helpers;

/// <summary>
/// Theme-aware highlighting definition that provides different syntax colors for light and dark themes
/// </summary>
public class ThemeAwareHighlightingDefinition : IHighlightingDefinition
{
    private readonly IHighlightingDefinition _lightThemeDefinition;
    private readonly IHighlightingDefinition _darkThemeDefinition;
    private ApplicationTheme _currentTheme;
    
    // Event that will be raised when the theme changes
    public event EventHandler? ThemeChanged;

    public ThemeAwareHighlightingDefinition(IHighlightingDefinition lightThemeDefinition, IHighlightingDefinition darkThemeDefinition)
    {
        _lightThemeDefinition = lightThemeDefinition;
        _darkThemeDefinition = darkThemeDefinition;
        _currentTheme = ApplicationThemeManager.GetAppTheme();
        
        // Register for theme changes
        ApplicationThemeManager.Changed += OnThemeChanged;
        
        // Debug output to confirm initialization
        System.Diagnostics.Debug.WriteLine($"ThemeAwareHighlightingDefinition initialized with current theme: {_currentTheme} (instance: {GetHashCode()})");
    }

    ~ThemeAwareHighlightingDefinition()
    {
        // Unregister to prevent memory leaks
        ApplicationThemeManager.Changed -= OnThemeChanged;
        System.Diagnostics.Debug.WriteLine($"ThemeAwareHighlightingDefinition disposed (instance: {GetHashCode()})");
    }

    private void OnThemeChanged(ApplicationTheme newTheme, Color accent)
    {
        if (_currentTheme != newTheme)
        {
            _currentTheme = newTheme;
            System.Diagnostics.Debug.WriteLine($"Theme changed to: {_currentTheme}, notifying subscribers (instance: {GetHashCode()})");
            
            // Notify subscribers that the theme has changed
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Forward all properties and methods to the appropriate theme definition
    private IHighlightingDefinition Current => 
        _currentTheme == ApplicationTheme.Light ? _lightThemeDefinition : _darkThemeDefinition;

    public string Name => Current.Name;
    public IEnumerable<HighlightingColor> NamedHighlightingColors => Current.NamedHighlightingColors;
    public IDictionary<string, string> Properties => Current.Properties;
    public HighlightingRuleSet MainRuleSet => Current.MainRuleSet;
    public HighlightingRuleSet GetNamedRuleSet(string name) => Current.GetNamedRuleSet(name);
    public HighlightingColor GetNamedColor(string name) => Current.GetNamedColor(name);
}

public static class AvalonEditHelper
{
    // Instead of using lazy initialization, we'll create a new instance every time
    // This ensures each editor gets its own instance that properly tracks theme changes
    public static IHighlightingDefinition AssemblyHighlightDefinition
    {
        get
        {
            // Always create a fresh instance to ensure proper theme handling
            return LoadAssembly16BitHighlighting();
        }
    }

    private static IHighlightingDefinition LoadAssembly16BitHighlighting()
    {
        IHighlightingDefinition lightThemeDefinition = null!;
        IHighlightingDefinition darkThemeDefinition = null!;
        
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string lightThemePath = Path.Combine(baseDir, "Assets", "AvalonEdit", "Assembly16BitHighlighting-Light.xshd");
            string darkThemePath = Path.Combine(baseDir, "Assets", "AvalonEdit", "Assembly16BitHighlighting-Dark.xshd");
            
            System.Diagnostics.Debug.WriteLine($"Looking for light theme at: {lightThemePath}");
            System.Diagnostics.Debug.WriteLine($"Looking for dark theme at: {darkThemePath}");
            
            if (File.Exists(lightThemePath) && File.Exists(darkThemePath))
            {
                using (var reader = XmlReader.Create(lightThemePath))
                {
                    lightThemeDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
                
                using (var reader = XmlReader.Create(darkThemePath))
                {
                    darkThemeDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
                
                // Use a unique variable name to avoid CS0136
                var themeAwareHighlightDefinition = new ThemeAwareHighlightingDefinition(lightThemeDefinition, darkThemeDefinition);
                
                System.Diagnostics.Debug.WriteLine("Successfully loaded theme-specific highlighting definitions.");
                return themeAwareHighlightDefinition;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not find theme-specific files:");
                if (!File.Exists(lightThemePath))
                    System.Diagnostics.Debug.WriteLine($"  - Light theme file not found: {lightThemePath}");
                if (!File.Exists(darkThemePath))
                    System.Diagnostics.Debug.WriteLine($"  - Dark theme file not found: {darkThemePath}");
            }
            
            System.Diagnostics.Debug.WriteLine("Trying with relative paths...");
            using (var reader = XmlReader.Create(@"Assets\AvalonEdit\Assembly16BitHighlighting-Light.xshd"))
            {
                lightThemeDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            
            using (var reader = XmlReader.Create(@"Assets\AvalonEdit\Assembly16BitHighlighting-Dark.xshd"))
            {
                darkThemeDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            
            // Create a highlighting definition for relative paths
            var relativePathsDefinition = new ThemeAwareHighlightingDefinition(lightThemeDefinition, darkThemeDefinition);
            System.Diagnostics.Debug.WriteLine("Successfully loaded theme-specific highlighting definitions with relative paths.");
            return relativePathsDefinition;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading theme-specific highlighting: {ex.Message}. Falling back to default.");
            
            try
            {
                using var reader = XmlReader.Create(@"Assets\AvalonEdit\Assembly16BitHighlighting.xshd");
                return HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            catch (Exception innerEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load default highlighting: {innerEx.Message}");
                throw;
            }
        }
    }
}