using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ui.Controllers.Monaco;
using Ui.ViewModels.Pages;
using Ui.ViewModels.Pages.Monaco;
using Ui.ViewModels.Windows;
using Ui.Views.Pages;
using Ui.Views.Windows;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;
using WorkspaceViewModel = Ui.ViewModels.WorkspaceViewModel;

namespace Ui;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // ReSharper disable once InconsistentNaming
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new InvalidOperationException());
        })
        .ConfigureServices((context, services) =>
        {
            services.AddNavigationViewPageProvider();

            services.AddLogging(builder => { builder.AddConsole(); });
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ITaskBarService, TaskBarService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IMonacoController, MonacoController>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            services.AddSingleton<WorkspaceViewModel>();

            services.AddSingleton<DashboardPage>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<DataPage>();
            services.AddSingleton<DataViewModel>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<AvalonEditPage>();
            services.AddSingleton<AvalonEditViewModel>();

            services.AddSingleton<MonacoPage>();
            services.AddSingleton<IMonacoViewModel, MonacoViewModel>();
        }).Build();
    
    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        base.OnExit(e);
    }

}

