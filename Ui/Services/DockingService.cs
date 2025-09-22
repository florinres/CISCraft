using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Models.Layouts;
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

        try
        {
            var anchorable = _dockingManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .FirstOrDefault(a => a.Content == tool);

            if (anchorable == null)
            {
                // If the anchorable isn't found, try to show the tool again
                ShowTool(tool);
                return;
            }

            if (tool.IsVisible)
                anchorable.Show();
            else
                anchorable.Hide();
        }
        catch (Exception ex)
        {
            // Handle the exception gracefully
            System.Diagnostics.Debug.WriteLine($"Error toggling visibility: {ex.Message}");
            // Try to show the tool again
            ShowTool(tool);
        }
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

    private const string layoutsFolderPath = "Layouts";

    public void SaveLayout(string fileName)
    {
        string filePath = layoutsFolderPath + $"\\{fileName}.layout";
        string metaPath = layoutsFolderPath + $"\\{fileName}.layoutmeta";

        var serializer = new XmlLayoutSerializer(_dockingManager);
        using (var stream = new StreamWriter(filePath))
        {
            serializer.Serialize(stream);
        }


        var meta = new LayoutMetadata();

        foreach (var tool in _activeDocumentService.Tools)
        {
            meta.Tools.Add(new ToolMetadata
            {
                Title = tool.Title,
                ZoomFactor = tool.ZoomFactor,
                IsVisible = tool.IsVisible
            });
        }

        var metaSerializer = new XmlSerializer(typeof(LayoutMetadata));
        using (var stream = new StreamWriter(metaPath))
        {
            metaSerializer.Serialize(stream, meta);
        }
    }

    public void LoadLayout(string fileName)
    {
        string layoutPath = layoutsFolderPath + $"\\{fileName}.layout";
        string metaPath = layoutsFolderPath + $"\\{fileName}.layoutmeta";

        if (!File.Exists(layoutPath) || _dockingManager?.Layout == null)
            return;

        LayoutMetadata? meta = null;
        if (File.Exists(metaPath))
        {
            var metaSerializer = new XmlSerializer(typeof(LayoutMetadata));
            using (var stream = new StreamReader(metaPath))
            {
                meta = (LayoutMetadata?)metaSerializer.Deserialize(stream);
            }
        }

        var serializer = new XmlLayoutSerializer(_dockingManager);
        serializer.LayoutSerializationCallback += (s, args) =>
        {
            var anchorable = args.Model as LayoutAnchorable;
            var title = anchorable?.Title;
            var tool = TryFindToolByTitle(title);

            if (tool != null)
            {
                args.Content = tool;

                var toolMeta = meta?.Tools.FirstOrDefault(t => t.Title == title);
                if (toolMeta != null)
                {
                    tool.ZoomFactor = toolMeta.ZoomFactor;
                    tool.IsVisible = toolMeta.IsVisible;
                }
            }
            else
            {
                args.Cancel = true;
            }
        };

        using var reader = new StreamReader(layoutPath);
        serializer.Deserialize(reader);
    }

    public void DeleteLayout(string fileName)
    {
        string filePath = Path.Combine(layoutsFolderPath, $"\\{fileName}.layout");
        string metaPath = layoutsFolderPath + $"\\{fileName}.layoutmeta";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        if (File.Exists(metaPath))
        {
            File.Delete(metaPath);
        }
    }

    public List<string> GetAllLayoutNames()
    {
        if (!Directory.Exists(layoutsFolderPath))
        {
            Directory.CreateDirectory(layoutsFolderPath);
            return [];
        }
        return Directory
            .GetFiles(layoutsFolderPath)
            .Select(file => Path.GetFileName(file))
            .Where(file => !string.IsNullOrWhiteSpace(file) && file.EndsWith(".layout"))
            .Select(file => file.Replace(".layout", ""))
            .ToList();
    }

    private const string LastUsedLayoutName = "__last_used";

    public void SaveLastUsedLayout()
    {
        SaveLayout(LastUsedLayoutName);
    }

    public void LoadLastUsedLayout()
    {
        if (GetAllLayoutNames().Contains(LastUsedLayoutName))
            LoadLayout(LastUsedLayoutName);
    }
    
    public void RestoreDefaultLayout()
    {
        try
        {
            // Delete the last used layout to force the application to use its default state
            if (GetAllLayoutNames().Contains(LastUsedLayoutName))
            {
                DeleteLayout(LastUsedLayoutName);
            }
            
            // Reset all tool visibility to their default state
            foreach (var tool in _activeDocumentService.Tools)
            {
                tool.IsVisible = true; // Default all tools to visible
                tool.ZoomFactor = 1.0; // Reset zoom to default
            }
            
            // Update the layout to reflect the changes
            _dockingManager?.UpdateLayout();
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the application
            System.Diagnostics.Debug.WriteLine($"Error restoring default layout: {ex.Message}");
            
            // At minimum, try to reset tool states even if file operations fail
            try
            {
                foreach (var tool in _activeDocumentService.Tools)
                {
                    tool.IsVisible = true;
                    tool.ZoomFactor = 1.0;
                }
            }
            catch (Exception innerEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting tool states: {innerEx.Message}");
            }
        }
    }
}