using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Services;

namespace Ui.ViewModels.Components.MenuBar;
public partial class LayoutControlViewModel : ObservableObject, ILayoutControlViewModel
{
    private readonly IToolVisibilityService _toolVisibilityService;
    private IDockingService _dockingService;

    public LayoutControlViewModel(IToolVisibilityService toolVisibilityService)
    {
        _toolVisibilityService = toolVisibilityService;
    }
    public void SetDockingService(IDockingService dockingService)
    {
        _dockingService = dockingService;
        SetLayouts();
    }
    private void SetLayouts()
    {
        if (_dockingService.GetType() != typeof(DummyDockingService))
        {
            LayoutNames.Clear();
            var layoutNames = _dockingService.GetAllLayoutNames();

            foreach (var layoutName in layoutNames)
            {
                LayoutNames.Add(layoutName);
            }
        }
    }

    public ObservableCollection<string> LayoutNames { get; set; } = [];
    [ObservableProperty] public partial string SelectedLayout { get; set; }
    
    [ObservableProperty]
    public partial string LayoutName { get; set; } = string.Empty;
    
    [RelayCommand(CanExecute = nameof(CanSaveLayout))]
    public void SaveLayout(string layoutName)
    {
        Directory.CreateDirectory("Layouts");

        _dockingService.SaveLayout(layoutName);
        SetLayouts();
        LayoutName = string.Empty;
    }
    
    private bool CanSaveLayout()
    {
        return !string.IsNullOrWhiteSpace(LayoutName);
    }

    
    [RelayCommand(CanExecute = nameof(CanLoadLayout))]
    public void LoadLayout(string layoutName)
    {
        _dockingService.LoadLayout(layoutName);
    }
    private bool CanLoadLayout() => true;
    
    [RelayCommand]
    public void LoadLayouts()
    {
        LayoutNames.Clear();
        foreach (var name in _dockingService.GetAllLayoutNames())
            LayoutNames.Add(name);
    }

    [RelayCommand]
    public void DeleteLayout(string layoutName)
    {
        _dockingService.DeleteLayout(layoutName);
        SetLayouts();
    }

    [RelayCommand]
    public void RestoreDefaultLayout()
    {
        _dockingService.RestoreDefaultLayout();
        SetLayouts();
    }
}