using System.ComponentModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel
{
    public enum StepLevel
    {
        Microcommand,
        Microinstruction,
        Instruction
    }
    public interface IActionsBarViewModel
    {
        /// <summary>
        ///     Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping
        ///     <see cref="ActionsBarViewModel.RunAssembleSourceCodeService" />.
        /// </summary>
        IAsyncRelayCommand RunAssembleSourceCodeServiceCommand { get; }
        public StepLevel StepLevel { get; set; }

        /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.Step"/>.</summary>
        global::CommunityToolkit.Mvvm.Input.IRelayCommand StepCommand { get; }
        /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.StartDebug"/>.</summary>
        global::CommunityToolkit.Mvvm.Input.IRelayCommand StartDebugCommand { get; }
        /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.StopDebug"/>.</summary>
        global::CommunityToolkit.Mvvm.Input.IRelayCommand StopDebugCommand { get; }

        /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IAsyncRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.LoadJson"/>.</summary>
        global::CommunityToolkit.Mvvm.Input.IAsyncRelayCommand LoadJsonCommand { get; }
        /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IAsyncRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.ResetProgram"/>.</summary>
        global::CommunityToolkit.Mvvm.Input.IRelayCommand ResetProgramCommand { get; }

        /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.SetStepLevel"/>.</summary>
        global::CommunityToolkit.Mvvm.Input.IRelayCommand<StepLevel> SetStepLevelCommand { get; }
        bool IsDebugging { get; set; }
        bool CanDebug { get; set; }
        bool CanAssemble { get; set; }

        event EventHandler<byte[]>? ObjectCodeGenerated;

        event PropertyChangedEventHandler? PropertyChanged;
        event PropertyChangingEventHandler? PropertyChanging;
    }
}