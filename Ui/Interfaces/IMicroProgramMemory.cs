using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Interfaces
{
    interface IMicroProgramMemory
    {
        public Dictionary<string, ulong> GetMicroProgramMemory();
        public void EditMicroRoutine();
        public void LoadMicroProgram();
    }
}
