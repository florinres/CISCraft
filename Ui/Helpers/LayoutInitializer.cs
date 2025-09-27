using AvalonDock.Layout;
using Ui.ViewModels.Generics;

namespace Ui.Helpers;

internal class LayoutInitializer : ILayoutUpdateStrategy
{
    public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow,
        ILayoutContainer destinationContainer)
    {
        try
        {
            // Safety check: if destinationContainer is null or invalid, use fallback
            if (destinationContainer == null)
            {
                return UseToolsPaneFallback(layout, anchorableToShow);
            }

    // If the pane is floating, let the manager handle it
    var floatingWindow = destinationContainer.FindParent<LayoutFloatingWindow>();
    if (floatingWindow != null)
    {
        return false; // Let AvalonDock handle floating windows
    }

            return UseToolsPaneFallback(layout, anchorableToShow);
        }
        catch (Exception ex)
        {
            // Log the error and use fallback
            System.Diagnostics.Debug.WriteLine($"Error in BeforeInsertAnchorable: {ex.Message}");
            return UseToolsPaneFallback(layout, anchorableToShow);
        }
    }

    private bool UseToolsPaneFallback(LayoutRoot layout, LayoutAnchorable anchorableToShow)
    {
        try
        {
            var toolsPane = layout.Descendents()
                .OfType<LayoutAnchorablePane>()
                .FirstOrDefault(d => d.Name == "ToolsPane");
            
            if (toolsPane != null)
            {
                toolsPane.Children.Add(anchorableToShow);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in UseToolsPaneFallback: {ex.Message}");
        }
        
        return false; // Let AvalonDock handle it as last resort
    }

    public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorable)
    {
        try
        {
            if (anchorable.Content is not ToolViewModel toolVm) return;

            toolVm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ToolViewModel.IsVisible)) 
                {
                    try
                    {
                        UpdateVisibility();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating visibility: {ex.Message}");
                    }
                }
            };

            // Set initial state
            UpdateVisibility();
            return;

            void UpdateVisibility()
            {
                if (toolVm.IsVisible)
                    anchorable.Show();
                else
                    anchorable.Hide();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AfterInsertAnchorable: {ex.Message}");
        }
    }


    public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow,
        ILayoutContainer destinationContainer)
    {
        return false;
    }

    public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
    {
    }
}