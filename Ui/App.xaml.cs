using System.IO;
using CPU.Business;
using CPU.Business.Models;
using MainMemory.Business;
using MainMemory.Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Services;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Components.HexViewer;
using Ui.ViewModels.Components.MenuBar;
using Ui.ViewModels.Components.Microprogram;
using Ui.ViewModels.Generics;
using Ui.ViewModels.Windows;
using Ui.Views.Windows;
using Wpf.Ui;
using WorkspaceViewModel = Ui.ViewModels.WorkspaceViewModel;
using ASMBLR = Assembler.Business.Assembler;
using MenuBarViewModel = Ui.ViewModels.Components.MenuBar.MenuBarViewModel;

namespace Ui;

/// <summary>
///     Interaction logic for App.xaml
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
        .ConfigureServices((_, services) =>
        {
            services.AddLogging(builder => { builder.AddConsole(); });
            services.AddSingleton<IThemeService, ThemeService>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<IMainWindowViewModel, MainWindowViewModel>();

            services.AddSingleton<IActiveDocumentService, ActiveDocumentService>();
            services.AddSingleton<IWorkspaceViewModel, WorkspaceViewModel>();
            services.AddSingleton<FileStatsViewModel>();

            services.AddSingleton<IActionsBarViewModel, ActionsBarViewModel>();
            services.AddSingleton<IMenuBarViewModel, MenuBarViewModel>();
            services.AddSingleton<IAssemblerService, AssemblerService>();
            services.AddSingleton<FileViewModel>();
            services.AddSingleton<ASMBLR>();
            services.AddSingleton<IDockingService, DummyDockingService>();
            services.AddSingleton<IToolVisibilityService, ToolVisibilityService>();
            services.AddSingleton<IDiagramViewModel, DiagramViewModel>();
            services.AddSingleton<IHexViewModel, HexViewModel>();
            services.AddSingleton<ISettingsViewModel, SettingsViewModel>();
            services.AddSingleton<ICpuService, CpuService>();
            services.AddSingleton<IMicroprogramViewModel, MicroprogramViewModel>();
            services.AddSingleton<IMainMemory, MainMemory.Business.MainMemory>();
            services.AddSingleton<CPU.Business.CPU>();
            services.AddSingleton<RegisterWrapper>();
            services.AddSingleton<MemoryContentWrapper>();
            services.AddSingleton<ControlUnit>();
            services.AddSingleton<ILayoutControlViewModel, LayoutControlViewModel>();
        }).Build();

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // var testWindow = new TestHexWindow();
        // testWindow.Show();
        
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        base.OnExit(e);
    }
}