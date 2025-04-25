using System.Collections.ObjectModel;
using Microsoft.Win32;
using Ui.Interfaces.Services;
using Ui.Interfaces.Windows;
using Ui.ViewModels.Generics;
using FileViewModel = Ui.ViewModels.Generics.FileViewModel;

namespace Ui.ViewModels;

public partial class WorkspaceViewModel : ObservableObject, IWorkspaceViewModel { }