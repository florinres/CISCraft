using System.Windows.Controls;
using AvalonDock.Layout;
using Ui.Models;

namespace Ui.Components;

public class PanesTemplateSelector : DataTemplateSelector
{
    public DataTemplate FileViewTemplate
    {
        get;
        set;
    }

    public DataTemplate FileStatsViewTemplate
    {
        get;
        set;
    }

    public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
    {
        var itemAsLayoutContent = item as LayoutContent;

        if (item is FileViewModel)
            return FileViewTemplate;

        // if (item is FileStatsViewModel)
        //     return FileStatsViewTemplate;

        return base.SelectTemplate(item, container);
    }
}