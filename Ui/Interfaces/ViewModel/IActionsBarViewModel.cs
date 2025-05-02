using System.ComponentModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;

public interface IActionsBarViewModel
{
    /// <summary>
    ///     Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand" /> instance wrapping
    ///     <see cref="ActionsBarViewModel.RunAssembleSourceCodeService" />.
    /// </summary>
    IRelayCommand RunAssembleSourceCodeServiceCommand { get; }

    event EventHandler<byte[]>? ObjectCodeGenerated;

    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}