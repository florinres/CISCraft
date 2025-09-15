using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace Ui.ViewModels.Components.Diagram;

public partial class HighlightableConnectorViewModel : BaseDiagramObject
{
    public HighlightableConnectorViewModel(string name)
    {
        Name = name;

        // defaults
        stroke = Brushes.White;
        hoverStroke = new SolidColorBrush(Color.FromRgb(0, 208, 255)); // #00D0FF
        thickness = 2;
        hoverThickness = 3;

        glowColor = Color.FromRgb(0, 208, 255);
        glowBlur = 14;
        glowOpacity = 0.9;

        zIndex = 1;
        tooltip = name;

        // initialize effective values
        effectiveStroke = stroke;
        effectiveThickness = thickness;
        effectiveGlowOpacity = 0;
    }

    // ---- identity/metadata ----
    [ObservableProperty] private string name;
    [ObservableProperty] private string? tooltip;

    // ---- base appearance ----
    [ObservableProperty] private Brush stroke;
    [ObservableProperty] private double thickness;

    // ---- hover appearance ----
    [ObservableProperty] private Brush hoverStroke;
    [ObservableProperty] private double hoverThickness;

    // ---- glow (used by view to bind DropShadowEffect) ----
    [ObservableProperty] private Color glowColor;
    [ObservableProperty] private double glowBlur;
    [ObservableProperty] private double glowOpacity;

    // ---- runtime state ----
    [ObservableProperty] private bool isHovered;
    [ObservableProperty] private int zIndex;

    // ---- effective values the view binds to directly ----
    [ObservableProperty] private Brush effectiveStroke;
    [ObservableProperty] private double effectiveThickness;
    [ObservableProperty] private double effectiveGlowOpacity;

    // When hover state flips, update effective visuals + ZIndex
    partial void OnIsHoveredChanged(bool value)
    {
        EffectiveStroke = value ? HoverStroke : Stroke;
        EffectiveThickness = value ? HoverThickness : Thickness;
        EffectiveGlowOpacity = value ? GlowOpacity : 0;
        ZIndex = value ? 10000 : 1;
    }

    // Keep effective values in sync if base/hover props change at runtime
    partial void OnStrokeChanged(Brush value)
    {
        if (!IsHovered) EffectiveStroke = value;
    }

    partial void OnHoverStrokeChanged(Brush value)
    {
        if (IsHovered) EffectiveStroke = value;
    }

    partial void OnThicknessChanged(double value)
    {
        if (!IsHovered) EffectiveThickness = value;
    }

    partial void OnHoverThicknessChanged(double value)
    {
        if (IsHovered) EffectiveThickness = value;
    }

    partial void OnGlowOpacityChanged(double value)
    {
        if (IsHovered) EffectiveGlowOpacity = value;
    }
}
