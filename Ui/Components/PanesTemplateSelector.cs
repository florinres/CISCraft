using System.Windows.Controls;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Generics;

namespace Ui.Components;

public class PanesTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FileViewTemplate { get; set; }
    public DataTemplate? FileStatsViewTemplate { get; set; }
    public DataTemplate? DiagramViewTemplate { get; set; }
    public DataTemplate? HexViewTemplate { get; set; }
    public DataTemplate? MicroprogramViewTemplate { get; set; }


    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            FileViewModel => FileViewTemplate,
            FileStatsViewModel => FileStatsViewTemplate,
            IDiagramViewModel => DiagramViewTemplate,
            IHexViewModel => HexViewTemplate,
            IMicroprogramViewModel => MicroprogramViewTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}