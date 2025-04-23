using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Contracts;

namespace Ui.Services
{
    public class AssemblerService : IAssemblerService
    {
        public void AssembleService(Assembler.Business.Assembler assembler, string sourceCode, string objectCodeFileName)
        {
            int len = 0;
            byte[] objectCode = assembler.Assemble(sourceCode, out len);

            using (FileStream fs = new FileStream(objectCodeFileName, FileMode.Create))
            {
                fs.Write(objectCode, 0, len);
            }
        }
    }
}
