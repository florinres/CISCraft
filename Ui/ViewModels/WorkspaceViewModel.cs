using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Services;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IWorkspaceViewModel
{
    public WorkspaceViewModel(IActiveDocumentService activeDocumentsService, IDockingService dockingService)
    {
        ActiveDocumentsService = activeDocumentsService;
    }

    [ObservableProperty] public partial IActiveDocumentService ActiveDocumentsService { get; set; }

    [RelayCommand]
    public void ShowFileStatsCommand()
    {
        ActiveDocumentsService.ToggleToolVisibility(ActiveDocumentsService.FileStats);
    }
}