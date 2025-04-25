using AvalonDock.Layout;
using Ui.ViewModels.Generics;

namespace Ui.Helpers;

class LayoutInitializer : ILayoutUpdateStrategy
{
    public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
    {
        //AD wants to add the anchorable into destinationContainer
        //just for test provide a new anchorablepane 
        //if the pane is floating let the manager go ahead
        LayoutAnchorablePane? destPane = destinationContainer as LayoutAnchorablePane;
        if (destinationContainer.FindParent<LayoutFloatingWindow>() != null)
            return false;

        var toolsPane = layout.Descendents()
                              .OfType<LayoutAnchorablePane>()
                              .FirstOrDefault(d => d.Name == "ToolsPane");
        if (toolsPane == null) return false;
        
        toolsPane.Children.Add(anchorableToShow);
        return true;

    }


    public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorable)
    {
        if (anchorable.Content is not ToolViewModel toolVm) return;

        toolVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolViewModel.IsVisible))
            {
                UpdateVisibility();
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


    public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
    {
        return false;
    }

    public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
    {

    }
}
