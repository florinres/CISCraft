using System.ComponentModel;

namespace Ui.Interfaces.ViewModel;

public interface IToolViewModel
{
    /// <inheritdoc />
    bool IsVisible { get; set; }
    bool HasToolBeenLoaded { get; set; }
    /// <inheritdoc />
    string Title { get; set; }
    /// <inheritdoc />
    string? ContentId { get; set; }
    /// <inheritdoc />
    bool IsSelected { get; set; }
    /// <inheritdoc />
    bool IsActive { get; set; }
    
    public double ZoomFactor { get; set; }
    
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}