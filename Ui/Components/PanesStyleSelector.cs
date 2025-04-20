using System.Windows.Controls;
using Ui.Models;

namespace Ui.Components;

public class PanesStyleSelector : StyleSelector
{
    public Style ToolStyle
    {
        get;
        set;
    }

    public Style FileStyle
    {
        get;
        set;
    }

    public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
    {
        // if (item is ToolViewModel)
        //     return ToolStyle;

        if (item is FileViewModel)
            return FileStyle;

        return base.SelectStyle(item, container);
    }
}