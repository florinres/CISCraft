using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Models;
using Ui.Models.Generics;

namespace Ui.ViewModels;

public interface IWorkspaceViewModel
{
    ObservableCollection<FileViewModel> Documents { get; }
    ObservableCollection<ToolViewModel> Tools { get; }
    FileViewModel? SelectedDocument { get; set; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="WorkspaceViewModel.AssembleSelectedFile"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand AssembleSelectedFileCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="WorkspaceViewModel.NewDocument"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand NewDocumentCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="WorkspaceViewModel.OpenDocument"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand OpenDocumentCommand { get; }

    /// <inheritdoc cref="WorkspaceViewModel._fileStats"/>
    global::Ui.Models.FileStatsViewModel FileStats
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_fileStats")]
        set;
    }

    void AssembleSelectedFile();
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}