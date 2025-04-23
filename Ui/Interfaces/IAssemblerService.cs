using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Contracts
{
    public interface IAssemblerService
    {
        void AssembleService(Assembler.Business.Assembler assembler, string sourceCode, string objectCodeFileName);
    }
}
