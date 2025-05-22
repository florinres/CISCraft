using System.Collections.ObjectModel;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Services;

public partial class ActiveDocumentService : ObservableObject, IActiveDocumentService
{
    private IDockingService _dockingService;

    public ActiveDocumentService(FileStatsViewModel fileStatsViewModel, IDockingService dockingService, IDiagramViewModel diagramViewModel, IHexViewModel hexViewModel, IMicroprogramViewModel microprogramViewModel)
    {
        FileStats = fileStatsViewModel;
        Diagram = diagramViewModel;
        HexViewer = hexViewModel;
        Microprogram = microprogramViewModel;
        
        Tools.Add(fileStatsViewModel);
        Tools.Add(diagramViewModel);
        Tools.Add(hexViewModel);
        Tools.Add(microprogramViewModel);

        _dockingService = dockingService;

        UpdateTools();
    }

    public FileViewModel? SelectedDocument
    {
        get;
        set
        {
            if (!SetProperty(ref field, value)) return;

            ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);
            UpdateTools();
        }
    }

    public ObservableCollection<FileViewModel> Documents { get; } = [];
    public ObservableCollection<IToolViewModel> Tools { get; } = [];

    [ObservableProperty] public partial FileStatsViewModel FileStats { get; set; }
    [ObservableProperty] public partial IDiagramViewModel Diagram { get; set; }
    
    [ObservableProperty] public partial IHexViewModel HexViewer { get; set; }
    [ObservableProperty] public partial IMicroprogramViewModel Microprogram { get; set; }

    public void ToggleToolVisibility(ToolViewModel tool)
    {
        if (tool.HasToolBeenLoaded == false)
        {
            _dockingService.ShowTool(tool);
            tool.HasToolBeenLoaded = true;
        }
    }

    public void SetDockingService(IDockingService dockingService)
    {
        _dockingService = dockingService;
    }


    public event EventHandler? ActiveDocumentChanged;

    private void UpdateTools()
    {
        FileStats.UpdateStats(SelectedDocument);
    }
}