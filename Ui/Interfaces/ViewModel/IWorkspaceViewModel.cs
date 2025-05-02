using System.ComponentModel;
using Ui.Interfaces.Services;

namespace Ui.Interfaces.ViewModel;

public interface IWorkspaceViewModel
{
    IActiveDocumentService ActiveDocumentsService { get; }
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;

    void ShowFileStatsCommand();
}