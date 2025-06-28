using System.IO;
using System.Text.Json;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Services;

public class DockingService : IDockingService
{
    private readonly DockingManager _dockingManager;
    private readonly IActiveDocumentService _activeDocumentService;

    public DockingService(DockingManager dockingManager, IActiveDocumentService activeDocumentService)
    {
        _dockingManager = dockingManager;
        _activeDocumentService = activeDocumentService;
    }

    public void ToggleVisibility(IToolViewModel tool)
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
    
    private IToolViewModel? TryFindToolByTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;

        return _activeDocumentService.Tools.FirstOrDefault(t => t.Title == title);
    }


    public void ShowTool(IToolViewModel tool)
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
    
    public void SaveLayout(string filePath)
    {
        if (_dockingManager == null) return;

        var serializer = new XmlLayoutSerializer(_dockingManager);
        using var stream = new StreamWriter(filePath);
        serializer.Serialize(stream);
    }
    
    public void LoadLayout(string filePath)
    {
        if (!File.Exists(filePath) || _dockingManager?.Layout == null)
            return;

        var serializer = new XmlLayoutSerializer(_dockingManager);

        serializer.LayoutSerializationCallback += (s, args) =>
        {
            var title = (args.Model as LayoutAnchorable)?.Title;
            var content = TryFindToolByTitle(title);
            if (content != null)
            {
                args.Content = content;
            }
            else
            {
                args.Cancel = true;
            }
        };


        using var reader = new StreamReader(filePath);
        serializer.Deserialize(reader);
    }

}