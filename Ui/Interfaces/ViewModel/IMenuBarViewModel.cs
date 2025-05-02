using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Interfaces.Services;
using Ui.Services;
using Ui.ViewModels;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;
public interface IMenuBarViewModel
{
    /// <inheritdoc/>
    IActiveDocumentService DocumentService { get; set; }
    
    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="MenuBarViewModel.OpenDocument"/>.</summary>
    IRelayCommand OpenDocumentCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="MenuBarViewModel.NewDocument"/>.</summary>
    IRelayCommand NewDocumentCommand { get; }
    
    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="MenuBarViewModel.NewDocument"/>.</summary>
    IRelayCommand ShowFileStatsCommand { get; }

    void SetDockingService(IDockingService dockingService);
    void SetToolsVisibilityOnAndOff();
}
