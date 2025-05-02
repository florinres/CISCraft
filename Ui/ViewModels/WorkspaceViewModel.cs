using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Microsoft.Win32;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Interfaces.Windows;
using Ui.ViewModels.Generics;
using FileViewModel = Ui.ViewModels.Generics.FileViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IWorkspaceViewModel
{

    [ObservableProperty]
    public partial IActiveDocumentService ActiveDocumentsService { get; set; }

    public WorkspaceViewModel(IActiveDocumentService activeDocumentsService)
    {
        ActiveDocumentsService = activeDocumentsService;
    }

    [RelayCommand]
    public void ShowFileStatsCommand()
    {
        ActiveDocumentsService.ToggleToolVisibility(ActiveDocumentsService.FileStats);
    }
}