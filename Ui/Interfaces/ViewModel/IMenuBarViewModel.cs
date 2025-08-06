using System.Collections.ObjectModel;
using Ui.Interfaces.Services;
using Ui.ViewModels.Components.MenuBar;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;

public interface IMenuBarViewModel
{
    ILayoutControlViewModel LayoutControl { get; set; }
    
    /// <inheritdoc />
    IActiveDocumentService DocumentService { get; set; }

    /// <summary>
    ///     Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping
    ///     <see cref="MenuBarViewModel.OpenDocument" />.
    /// </summary>
    IAsyncRelayCommand OpenDocumentCommand { get; }

    /// <summary>
    ///     Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping
    ///     <see cref="MenuBarViewModel.NewDocument" />.
    /// </summary>
    IRelayCommand NewDocumentCommand { get; }

    /// <summary>
    ///     Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping
    ///     <see cref="MenuBarViewModel.NewDocument" />.
    /// </summary>
    IRelayCommand ShowFileStatsCommand { get; }
    
    IRelayCommand ShowDiagramCommand { get; }
    
    IRelayCommand ShowHexViewerCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="MenuBarViewModel.ShowMicroprogram"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand ShowMicroprogramCommand { get; }

    void SetDockingService(IDockingService dockingService);
    void SetToolsVisibilityOnAndOff();
}