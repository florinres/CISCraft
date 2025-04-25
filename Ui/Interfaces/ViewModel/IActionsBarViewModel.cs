using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Interfaces.ViewModel;
public interface IActionsBarViewModel
{
    event EventHandler<byte[]>? ObjectCodeGenerated;
}
