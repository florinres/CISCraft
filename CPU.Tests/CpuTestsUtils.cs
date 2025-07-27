using System.Security.Cryptography;
using Cpu = CPU.Business.CPU;
using Ram = MainMemory.Business.MainMemory;
using MemWrapper = MainMemory.Business.Models.MomeryContentWrapper;
using ASM = Assembler.Business.Assembler;
using CPU.Business.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace CPU.Tests
{
    public class CpuTestsUtils
    {
        internal List<string> GetInstructionPath(Cpu cpu)
        {
            int a, b, i;
            a = b = i = 0;
            string previousLabel = "";
            List<string> realPath = new List<string>();

            (a, b) = cpu.StepMicrocommand();
            i++;
            (a, b) = cpu.StepMicrocommand();
            i++;
            while ((a != 0) || (b != 0))
            {
                (a, b) = cpu.StepMicrocommand();
                string currentLabel = cpu.GetCurrentLabel(a);
                if (previousLabel != currentLabel)
                    realPath.Add(currentLabel);
                i++;

                previousLabel = currentLabel;
            }
            realPath.RemoveAt(realPath.Count-1);
            return realPath;
        }

        // TODO: https://github.com/users/florinres/projects/2?pane=issue&itemId=119368760&issue=florinres%7CCISCraft%7C52
        public void GenerateTraceLog(/*TO BE DEFINED*/)
        {

        }
    }
}

