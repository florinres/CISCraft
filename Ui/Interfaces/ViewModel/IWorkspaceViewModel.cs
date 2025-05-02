using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Interfaces.Services;
using Ui.ViewModels;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;

public interface IWorkspaceViewModel
{
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
    IActiveDocumentService ActiveDocumentsService { get; }

    void ShowFileStatsCommand();
}