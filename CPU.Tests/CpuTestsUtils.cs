using Cpu = CPU.Business.CPU;
using Ram = MainMemory.Business.MainMemory;
using MemWrapper = MainMemory.Business.Models.MemoryContentWrapper;
using ASM = Assembler.Business.Assembler;
using CPU.Business.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using CPU.Business;

namespace CPU.Tests
{
    public static class CpuTestsUtils
    {
        public const short stackPointer = 0xA;
        const short MaxMemoryDump = 20;
        public static Dictionary<string, bool> CoveredMpm = new Dictionary<string, bool>();
        internal static void CapturePathAndRegisters(Cpu cpu, List<KeyValuePair<string, string>> realPath, List<Dictionary<string, int>> registerSnapshots)
        {
            int a, b, i;

            while (!IsIrEmpty(cpu))
            {
                (a, b) = cpu.StepMicrocommand();
                var buf = cpu.GetCurrentLabel(a);
                string currentLabel = buf.Item1;
                string microinstruction = "";
                foreach (var label in buf.Item2)
                {
                    microinstruction += label + " ";
                }
                var pathBuffer = new KeyValuePair<string, string>(currentLabel, microinstruction);
                if (b == 6)
                {
                    CaptureRegisterSnapshot(cpu, registerSnapshots);
                    realPath.Add(pathBuffer);
                }
            }
            cpu.Registers[REGISTERS.PC] -= 2;
        }

        public static void GenerateTraceLog(
            byte[] beforeDump,
            byte[] afterDump,
            List<Dictionary<string, int>> registerSnapshots,
            List<string> expectedPath,
            List<KeyValuePair<string, string>> realPath,
            string testName,
            string instruction,
            string fileName)
        {
            string currentFolder = Path.GetFullPath(AppContext.BaseDirectory + "../../../");
            string outputFile = Path.Combine(currentFolder, fileName);

            File.AppendAllText(outputFile, $"Trace log for test: {testName}\n");
            File.AppendAllText(outputFile, $"{instruction}\n\n");

            File.AppendAllText(outputFile, "Expected Path: ");
            foreach (var label in expectedPath)
                File.AppendAllText(outputFile, label + " ");
            File.AppendAllText(outputFile, "\n");

            File.AppendAllText(outputFile, "Real Path:     ");
            foreach (var label in realPath)
                File.AppendAllText(outputFile, label.Key + " ");
            File.AppendAllText(outputFile, "\n\n");

            for (int i = 1; i < registerSnapshots.Count; i++)
            {
                Dictionary<string, int>? prev = registerSnapshots[i - 1];
                Dictionary<string, int>? curr = registerSnapshots[i];

                try
                {
                    File.AppendAllText(outputFile, realPath[i - 1].Key + "\n");
                    File.AppendAllText(outputFile, realPath[i - 1].Value + "\n\n");
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
                        File.AppendAllText(outputFile, $"  {key}: 0x{((ushort)prevVal).ToString("X")} -> 0x{((ushort)currVal).ToString("X")}\n");
                    }
                }

                File.AppendAllText(outputFile, "\n");
            }

            File.AppendAllText(outputFile, $"Dump before [0-{MaxMemoryDump}]:\n");
            foreach (byte i in beforeDump[0..MaxMemoryDump])
            {
                File.AppendAllText(outputFile, "0x"+ i.ToString("X") + " ");
            }

            File.AppendAllText(outputFile, "\n");

            File.AppendAllText(outputFile, $"Dump after [0-{MaxMemoryDump}]:\n");
            foreach (byte i in afterDump[0..MaxMemoryDump])
            {
                File.AppendAllText(outputFile, "0x" + i.ToString("X") + " ");
            }
            File.AppendAllText(outputFile, "\n\n");

        }

        // Captures the registers that changed after executing one microinstruction
        private static void CaptureRegisterSnapshot(Cpu cpu, List<Dictionary<string, int>> registerSnapshots)
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
            buf["SBUS"]  = cpu.SBUS;
            buf["DBUS"]  = cpu.DBUS;
            buf["RBUS"]  = cpu.RBUS;

            registerSnapshots.Add(buf);
        }

        private static bool IsIrEmpty(Cpu cpu)
        {
            cpu.StepMicrocommand();
            cpu.StepMicrocommand();
            cpu.StepMicrocommand();
            cpu.StepMicrocommand();
            cpu.StepMicrocommand();
            cpu.StepMicrocommand();
            return cpu.Registers[REGISTERS.IR] == 0;
        }
        public static void InitTest(List<Dictionary<string, int>> registerSnapshots, Cpu cpu)
        {
            Dictionary<string, int> buf = new Dictionary<string, int>();

            cpu.ACLOW = false;
            cpu.CIL = false;
            cpu.DBUS = 0;
            cpu.SBUS = 0;
            cpu.RBUS = 0;
            cpu.INT = false;

            for (int i = 0; i <= EnumExtensions.GetMaxValue<REGISTERS>(); i++)
            {
                cpu.Registers[(REGISTERS)i] = 0;
            }
            cpu.Registers[REGISTERS.ONES] = -1;
            cpu.Registers[REGISTERS.SP] = stackPointer;

            for (int i = 0; i < 16; i++)
            {
                buf["R" + i] = 0;
            }

            buf["None"]  = 0;
            buf["FLAGS"] = 0;
            buf["RG"]    = 0;
            buf["SP"]    = stackPointer;
            buf["T "]    = 0;
            buf["PC"]    = 0;
            buf["IVR"]   = 0;
            buf["ADR"]   = 0;
            buf["MDR"]   = 0;
            buf["IRLSB"] = 0;
            buf["NEG"]   = 0;
            buf["ZEROS"] = 0;
            buf["ONES"]  = -1;
            buf["IR"]    = 0;
            buf["SBUS"]  = 0;
            buf["DBUS"]  = 0;
            buf["RBUS"]  = 0;

            registerSnapshots.Add(buf);
        }
    }
}
