using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Interfaces.Services;
using Ui.ViewModels.Components.MenuBar;

namespace Ui.Interfaces.ViewModel;

public interface ILayoutControlViewModel
{
    void SetDockingService(IDockingService dockingService);
    ObservableCollection<string> LayoutNames { get; set; }

    string SelectedLayout { get; set; }
    string LayoutName { get; set; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand"/> instance wrapping <see cref="LayoutControlViewModel.LoadLayouts"/>.</summary>
    IRelayCommand LoadLayoutsCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand{T}"/> instance wrapping <see cref="LayoutControlViewModel.LoadLayout"/>.</summary>
    IRelayCommand<string> LoadLayoutCommand { get; }

    /// <summary>Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IRelayCommand{T}"/> instance wrapping <see cref="LayoutControlViewModel.SaveLayout"/>.</summary>
    IRelayCommand<string> SaveLayoutCommand { get; }
    
    IRelayCommand<string> DeleteLayoutCommand { get; }

    void SaveLayout(string layoutName);
    void LoadLayout(string layoutName);
    void LoadLayouts();
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}