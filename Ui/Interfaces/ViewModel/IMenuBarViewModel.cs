using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.ViewModels;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;
public interface IMenuBarViewModel
{
    ObservableCollection<FileViewModel> Documents { get; }
    ObservableCollection<ToolViewModel> Tools { get; }
    FileViewModel? SelectedDocument { get; set; }

    /// <inheritdoc cref="MenuBarViewModel._fileStats"/>
    global::Ui.ViewModels.Generics.FileStatsViewModel FileStats
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_fileStats")]
        set;
    }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="MenuBarViewModel.NewDocument"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand NewDocumentCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="MenuBarViewModel.OpenDocument"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand OpenDocumentCommand { get; }

    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}
