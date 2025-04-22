using System.Windows.Controls;
using AvalonDock.Layout;
using Ui.Models;

namespace Ui.Components;

public class PanesTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FileViewTemplate { get; set; }
    public DataTemplate? FileStatsViewTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            FileViewModel => FileViewTemplate,
            FileStatsViewModel => FileStatsViewTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}