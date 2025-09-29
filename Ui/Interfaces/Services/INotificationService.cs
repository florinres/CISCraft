using System.Windows;

namespace Ui.Interfaces.Services;

public interface INotificationService
{
    void ShowError(string message, int durationInSeconds = 3);
    void ShowInfo(string message, int durationInSeconds = 3);
    void ShowWarning(string message, int durationInSeconds = 3);
    void ShowSuccess(string message, int durationInSeconds = 3);
}