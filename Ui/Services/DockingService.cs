using AvalonDock;
using AvalonDock.Layout;
using Ui.Interfaces.Services;
using Ui.ViewModels.Generics;

namespace Ui.Services;

public class DockingService : IDockingService
{
    private readonly DockingManager _dockingManager;

    public DockingService(DockingManager dockingManager)
    {
        _dockingManager = dockingManager;
    }

    public void ToggleVisibility(ToolViewModel tool)
    {
        if (_dockingManager?.Layout == null)
            return;

        var anchorable = _dockingManager.Layout.Descendents()
            .OfType<LayoutAnchorable>()
            .First(a => a.Content == tool);

        if (tool.IsVisible)
            anchorable.Show();
        else
            anchorable.Hide();
    }

    public void ShowTool(ToolViewModel tool)
    {
        if (_dockingManager?.Layout == null)
            return;

        var anchorable = _dockingManager.Layout.Descendents()
            .OfType<LayoutAnchorable>()
            .FirstOrDefault(a => a.Content == tool);

        if (anchorable == null)
        {
            anchorable = new LayoutAnchorable
            {
                Content = tool
            };

            var pane = _dockingManager.Layout.Descendents()
                .OfType<LayoutAnchorablePane>()
                .FirstOrDefault(p => p.Name == "ToolsPane");

            pane?.Children.Add(anchorable);
            _dockingManager.UpdateLayout();

            anchorable.Show();
        }
    }
}