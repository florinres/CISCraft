using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlUnit
{
    interface IControlUnitStatus
    {
        void Run();       
        void Pause();     
        void Step();     
        void Reset();  

    }
}
