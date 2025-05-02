using System.ComponentModel;
using Wpf.Ui.Abstractions.Controls;

namespace Ui.Interfaces.ViewModel;

public interface IViewModel : INavigationAware
{
    public void OnNavigatedTo();

    /// <summary>
    ///     Handles the event that is fired before the component is navigated from.
    /// </summary>
    // ReSharper disable once MemberCanBeProtected.Global
    public void OnNavigatedFrom();

    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}