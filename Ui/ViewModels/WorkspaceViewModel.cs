using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IWorkspaceViewModel
{
    public WorkspaceViewModel(IActiveDocumentService activeDocumentsService)
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