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
        internal void CapturePathAndRegisters(Cpu cpu, List<string> realPath, List<Dictionary<string, int>> registerSnapshots)
        {
            int a, b, i;
            a = b = i = 0;
            string previousLabel = "";

            (a, b) = cpu.StepMicrocommand();
            i++;
            (a, b) = cpu.StepMicrocommand();
            i++;
            while ((a != 0) || (b != 0))
            {
                (a, b) = cpu.StepMicrocommand();
                string currentLabel = cpu.GetCurrentLabel(a);
                if (previousLabel != currentLabel)
                {
                    CaptureRegisterSnapshot(cpu, registerSnapshots);
                    realPath.Add(currentLabel);
                }
                i++;

                previousLabel = currentLabel;
            }
            realPath.RemoveAt(realPath.Count - 1);
        }

        // TODO: https://github.com/users/florinres/projects/2?pane=issue&itemId=119368760&issue=florinres%7CCISCraft%7C52
        public void GenerateTraceLog(List<Dictionary<string, int>> registerSnapshots, List<string> expectedPath,List<string> realPath, string testName)
        {
            var prev = registerSnapshots[0];
            var curr = registerSnapshots[1];
            for (int i = 2; i < registerSnapshots.Count; i++)
            {
                prev = registerSnapshots[i-1];
                curr = registerSnapshots[i];

                try
                {
                    File.WriteAllText("SnapShots.txt", realPath[i - 2] + "\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File write error: " + ex.Message);
                }
                foreach (var key in curr.Keys.Union(prev.Keys))
                {
                    prev.TryGetValue(key, out int prevVal);
                    curr.TryGetValue(key, out int currVal);

                    if (prevVal != currVal)
                    {
                        File.AppendAllText("SnapShots.txt", $"  {key}: {prevVal} -> {currVal}\n");
                    }
                }

                File.AppendAllText("SnapShots.txt", "\n");
            }
        }

        // Captures the registers that changed after executing one microinstruction
        private void CaptureRegisterSnapshot(Cpu cpu, List<Dictionary<string, int>> registerSnapshots)
        {
            Dictionary<string, int> buf = new Dictionary<string, int>();
            for (int i = 0; i < cpu.Registers.Gpr.Length; i++)
            {
                buf["R" + i] = cpu.Registers.Gpr[i];
            }

            buf["None"]  = cpu.Registers.Registers[0];
            buf["FLAGS"] = cpu.Registers.Registers[1];
            buf["RG"]    = cpu.Registers.Registers[2];
            buf["SP"]    = cpu.Registers.Registers[3];
            buf["T "]    = cpu.Registers.Registers[4];
            buf["PC"]    = cpu.Registers.Registers[5];
            buf["IVR"]   = cpu.Registers.Registers[6];
            buf["ADR"]   = cpu.Registers.Registers[7];
            buf["MDR"]   = cpu.Registers.Registers[8];
            buf["IRLSB"] = cpu.Registers.Registers[9];
            buf["NEG"]   = cpu.Registers.Registers[10];
            buf["ZEROS"] = cpu.Registers.Registers[11];
            buf["ONES"]  = cpu.Registers.Registers[12];
            buf["IR"]    = cpu.Registers.Registers[13];

            registerSnapshots.Add(buf);
        }

        public void InitRegisterSnapshots(List<Dictionary<string, int>> registerSnapshots)
        {
            Dictionary<string, int> buf = new Dictionary<string, int>();
            for (int i = 0; i < 16; i++)
            {
                buf["R" + i] = 0;
            }

            buf["None"]  = 0;
            buf["FLAGS"] = 0;
            buf["RG"]    = 0;
            buf["SP"]    = 0;
            buf["T "]    = 0;
            buf["PC"]    = 0;
            buf["IVR"]   = 0;
            buf["ADR"]   = 0;
            buf["MDR"]   = 0;
            buf["IRLSB"] = 0;
            buf["NEG"]   = 0;
            buf["ZEROS"] = 0;
            buf["ONES"]  = 0;
            buf["IR"]    = 0;

            registerSnapshots.Add(buf);
        }
    }
}

