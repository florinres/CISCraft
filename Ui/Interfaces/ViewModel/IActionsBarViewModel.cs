using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;
public interface IActionsBarViewModel
{
    event EventHandler<byte[]>? ObjectCodeGenerated;

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="ActionsBarViewModel.RunAssembleSourceCodeService"/>.</summary>
    global::CommunityToolkit.Mvvm.Input.IRelayCommand RunAssembleSourceCodeServiceCommand { get; }

    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}
