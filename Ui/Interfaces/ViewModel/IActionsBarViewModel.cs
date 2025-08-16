using System.ComponentModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;

public interface IActionsBarViewModel
{
    /// <summary>
    ///     Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping
    ///     <see cref="ActionsBarViewModel.RunAssembleSourceCodeService" />.
    /// </summary>
    IAsyncRelayCommand RunAssembleSourceCodeServiceCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.StepMicroprogram"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand StepMicroprogramCommand { get; }
    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.StartDebug"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand StartDebugCommand { get; }
    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.StopDebug"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand StopDebugCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IAsyncRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.LoadJson"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IAsyncRelayCommand LoadJsonCommand { get; }
    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IAsyncRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.ResetProgram"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand ResetProgramCommand { get; }

    bool IsDebugging { get; set; }
    bool NotDebugging { get; set; }

    event EventHandler<byte[]>? ObjectCodeGenerated;

    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}