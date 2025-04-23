using System.Windows.Controls;
using Ui.Models;
using Ui.Models.Generics;

namespace Ui.Components;

public class PanesStyleSelector : StyleSelector
{
    public Style? ToolStyle
    {
        get;
        set;
    }

    public Style? FileStyle
    {
        get;
        set;
    }

    public override Style SelectStyle(object item, DependencyObject container)
    {
        //using ! so it breaks if there is no style
        return item switch
        {
            ToolViewModel => ToolStyle!,
            FileViewModel => FileStyle!,
            _ => base.SelectStyle(item, container)!
        };
    }
}