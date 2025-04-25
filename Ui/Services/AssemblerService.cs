using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Interfaces.Services;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.Services
{
    public class AssemblerService : IAssemblerService
    {
        ASMBLR _assembler;
        public AssemblerService(ASMBLR assembler)
        {
            _assembler = assembler;
        }
        public byte[] AssembleSourceCodeService(string sourceCode)
        {
            int len = 0;
            byte[] objectCode = _assembler.Assemble(sourceCode, out len);
            return objectCode;
        }
    }
}
