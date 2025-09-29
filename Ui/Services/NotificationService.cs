using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ui.Interfaces.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

// Use aliases to resolve ambiguous references
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Ui.Services;

public class NotificationService : INotificationService
{
    // Notification type definitions
    private enum NotificationType
    {
        Error,
        Info,
        Warning, 
        Success
    }
    
    // Configuration for each notification type
    private readonly struct NotificationConfig
    {
        public string Title { get; init; }
        public string ColorHex { get; init; }
        public SymbolRegular IconSymbol { get; init; }
        public MessageBoxImage FallbackIcon { get; init; }
    }
    
    private static SnackbarPresenter? _snackbarPresenter;

    public static void SetSnackbarPresenter(SnackbarPresenter presenter)
    {
        _snackbarPresenter = presenter;
    }
    
    // Get configuration for different notification types
    private NotificationConfig GetConfig(NotificationType type) => type switch
    {
        NotificationType.Error => new NotificationConfig 
        { 
            Title = "Error", 
            ColorHex = "#FFE53935", 
            IconSymbol = SymbolRegular.ErrorCircle24,
            FallbackIcon = MessageBoxImage.Error
        },
        NotificationType.Info => new NotificationConfig 
        { 
            Title = "Information", 
            ColorHex = "#FF2196F3", 
            IconSymbol = SymbolRegular.Info24,
            FallbackIcon = MessageBoxImage.Information
        },
        NotificationType.Warning => new NotificationConfig 
        { 
            Title = "Warning", 
            ColorHex = "#FFFFC107", 
            IconSymbol = SymbolRegular.Warning24,
            FallbackIcon = MessageBoxImage.Warning
        },
        NotificationType.Success => new NotificationConfig 
        { 
            Title = "Success", 
            ColorHex = "#FF4CAF50", 
            IconSymbol = SymbolRegular.CheckmarkCircle24,
            FallbackIcon = MessageBoxImage.Information
        },
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
    
    // Private helper method to show notifications
    private void ShowNotification(string message, NotificationType type, int durationInSeconds)
    {
        var config = GetConfig(type);
        
        if (_snackbarPresenter == null)
        {
            // Fallback to standard MessageBox if presenter isn't available
            MessageBox.Show(message, config.Title, MessageBoxButton.OK, config.FallbackIcon);
            return;
        }
        
        Application.Current.Dispatcher.InvokeAsync(() => {
            try
            {
                // Create and show a snackbar using the WPF-UI API
                var snackbar = new Snackbar(_snackbarPresenter)
                {
                    Title = config.Title,
                    Content = message,
                    Icon = new SymbolIcon { Symbol = config.IconSymbol },
                    Timeout = TimeSpan.FromSeconds(durationInSeconds),
                    // Make the notification more compact
                    FontSize = 12,
                    Padding = new Thickness(10, 4, 10, 4)
                };
                
                // Create a compact content presenter for the message
                var contentTextBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 300,
                    Margin = new Thickness(0),
                    FontSize = 12
                };
                
                snackbar.Content = contentTextBlock;
                
                // Set appearance based on notification type
                switch (type)
                {
                    case NotificationType.Error:
                        snackbar.Appearance = ControlAppearance.Danger;
                        break;
                    case NotificationType.Warning:
                        snackbar.Appearance = ControlAppearance.Caution;
                        break;
                    case NotificationType.Success:
                        snackbar.Appearance = ControlAppearance.Success;
                        break;
                    case NotificationType.Info:
                    default:
                        snackbar.Appearance = ControlAppearance.Info;
                        break;
                }
                
                // Show notification using the built-in API
                snackbar.Show();
            }
            catch (Exception ex)
            {
                // In case of any issues, use standard MessageBox
                MessageBox.Show($"Error showing notification: {ex.Message}\n\nOriginal message: {message}", 
                    config.Title, MessageBoxButton.OK, config.FallbackIcon);
            }
        });
    }
    
    public void ShowError(string message, int durationInSeconds = 3)
    {
        ShowNotification(message, NotificationType.Error, durationInSeconds);
    }

    public void ShowInfo(string message, int durationInSeconds = 3)
    {
        ShowNotification(message, NotificationType.Info, durationInSeconds);
    }

    public void ShowWarning(string message, int durationInSeconds = 3)
    {
        ShowNotification(message, NotificationType.Warning, durationInSeconds);
    }

    public void ShowSuccess(string message, int durationInSeconds = 3)
    {
        ShowNotification(message, NotificationType.Success, durationInSeconds);
    }
}