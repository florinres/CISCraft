using System.Security.Cryptography;
using Cpu = CPU.Business.CPU;
using Ram = MainMemory.Business.MainMemory;
using MemWrapper = MainMemory.Business.Models.MemoryContentWrapper;
using ASM = Assembler.Business.Assembler;
using CPU.Business.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace CPU.Tests
{
    [TestClass]
    public class B4InstructionsTests
    {
        ASM? assembler;
        MemWrapper? memWrapper;
        Ram? ram;
        RegisterWrapper? list;
        Cpu? cpu;
        List<KeyValuePair<string, string>>? realInstructionPath;
        List<string>? expectedInstructionPath;
        const short UserCodeStartAddress = 16;

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            File.Delete(AppContext.BaseDirectory + "../../../SnapShots_B4.txt");
        }

        [TestInitialize]
        public void Setup()
        {
            assembler = new ASM();
            memWrapper = new MemWrapper();
            ram = new Ram(memWrapper);
            list = new RegisterWrapper(20);
            cpu = new Cpu(ram,list);
            realInstructionPath = new List<KeyValuePair<string, string>>();

            string jsonPath = Path.GetFullPath(AppContext.BaseDirectory + "../../../../Configs/MPM.json");
            string jsonString = File.ReadAllText(jsonPath);
            cpu.LoadJsonMpm(jsonString);
        }
        [TestCleanup]
        public void Cleanup()
        {
            if (realInstructionPath != null && expectedInstructionPath != null)
            {
                List<string> buf = new List<string>();
                foreach (var kvp in realInstructionPath)
                {
                    buf.Add(kvp.Key);
                }
                Assert.IsTrue(buf.SequenceEqual(expectedInstructionPath));
            }

            foreach (string label in expectedInstructionPath)
            {
                CpuTestsUtils.CoveredMpm[label] = true;
            }
        }

        private void RunInstructionTest(
            string testName,
            string sourceCode,
            List<string> expectedPath,
            Action postAssert)
        {
            int len;
            expectedInstructionPath = expectedPath;
            List<Dictionary<string, int>> registerSnapshots = new List<Dictionary<string, int>>();

            if (
                    (ram == null)           ||
                    (assembler == null)     ||
                    (cpu == null)           ||
                    (realInstructionPath == null)
                ) return;

            CpuTestsUtils.InitTest(registerSnapshots, cpu);

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));
            byte[] initRamDump = (byte[])ram.GetMemoryDump().Clone();
            
            CpuTestsUtils.CapturePathAndRegisters(cpu, realInstructionPath, registerSnapshots);

            CpuTestsUtils.GenerateTraceLog(initRamDump, ram.GetMemoryDump(), registerSnapshots, expectedInstructionPath, realInstructionPath, testName, sourceCode,"SnapShots_B4.txt");

            postAssert();
        }
        [TestMethod]
        public void CLC_TEST()
        {
            if (cpu == null || ram == null) return;

            cpu.Registers[GPR.R0] = 2;
            RunInstructionTest(
                "CLC_TEST",
                "cmp r0, 1\nclc",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "CMP_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "CLC_0",
                },
                () =>
                {
                    Assert.AreEqual(0, cpu.GetCarryFlag()); 
                });
        }
        [TestMethod]
        public void SEC_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "SEC_TEST",
                "sec",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "SEC_0",
                },
                () =>
                {
                    Assert.AreEqual(1, cpu.GetCarryFlag()); 
                });
        }
        // [TestMethod] // Keep it enabled only when testing
        public void HALT_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "HALT_TEST",
                "halt",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "HALT_0",
                    "HALT_1",
                    "EI_0"
                },
                () => { });
        }
        [TestMethod]
        public void NOP_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "NOP_TEST",
                "nop",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "NOP_0"
                },
                () => { });
        }
        [TestMethod]
        public void EI_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "EI_TEST",
                "ei",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "EI_0",
                    "EI_1"
                },
                () =>
                { 
                    Assert.AreEqual(1, cpu.GetInterruptFlag()); 
                });
        }
        [TestMethod]
        public void DI_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "DI_TEST",
                "ei\ndi",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "EI_0",
                    "EI_1",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "DI_0"
                },
                () =>
                { 
                    Assert.AreEqual(0, cpu.GetInterruptFlag()); 
                });
        }
        [TestMethod]
        public void PUSHPC_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "PUSHPC_TEST",
                "pushpc",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "PUSH PC_0",
                    "PUSH PC_1"
                },
                () =>
                { 
                    Assert.AreEqual(UserCodeStartAddress + 2, ram.FetchWord(CpuTestsUtils.stackPointer)); 
                });
        }
        [TestMethod]
        public void POPPC_TEST()
        {
            if (cpu == null || ram == null) return;

            cpu.Registers[GPR.R0] = UserCodeStartAddress + 12;
            RunInstructionTest(
                "POPPC_TEST",
                "push r0\npoppc",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "PUSH_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "POP PC_0",
                    "POP PC_1",
                    "POP PC_2"
                },
                () =>
                { 
                    Assert.AreEqual(UserCodeStartAddress + 12, cpu.Registers[REGISTERS.PC]); 
                });
        }
        [TestMethod]
        public void PUSHFLAG_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "PUSHFLAG_TEST",
                "sec\npushf",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "SEC_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "PUSH FLAG_0",
                    "PUSH FLAG_1",
                },
                () =>
                { 
                    Assert.AreEqual(0x8, cpu.Registers[REGISTERS.FLAGS]); 
                    Assert.AreEqual(0x8, ram.FetchWord(CpuTestsUtils.stackPointer)); 
                });
        }
        [TestMethod]
        public void POPFLAG_TEST()
        {
            if (cpu == null || ram == null) return;

            RunInstructionTest(
                "POPFLAG_TEST",
                "sec\npushf\nclc\npopf",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "SEC_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "PUSH FLAG_0",
                    "PUSH FLAG_1",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "CLC_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "POP FLAG_0",
                    "POP FLAG_1",
                    "POP FLAG_2",
                },
                () =>
                { 
                    Assert.AreEqual(0x8, cpu.Registers[REGISTERS.FLAGS]); 
                    Assert.AreEqual(0x8, ram.FetchWord(CpuTestsUtils.stackPointer)); 
                });
        }
        [TestMethod]
        public void RET_TEST()
        {
            if (cpu == null || ram == null) return;

            cpu.Registers[GPR.R0] = UserCodeStartAddress + 0x10;
            RunInstructionTest(
                "RET_TEST",
                "push r0\nret",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "PUSH_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "RET_0",
                    "RET_1",
                    "RET_2",
                },
                () =>
                { 
                    Assert.AreEqual(UserCodeStartAddress + 0x10, cpu.Registers[REGISTERS.PC]); 
                    Assert.AreEqual(CpuTestsUtils.stackPointer, cpu.Registers[REGISTERS.SP]); 
                });
        }
        [TestMethod]
        public void IRET_TEST()
        {
            if (cpu == null || ram == null) return;

            cpu.Registers[GPR.R0] = UserCodeStartAddress + 0x10;
            RunInstructionTest(
                "IRET_TEST",
                "sec\npushf\npush r0\niret",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "SEC_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "PUSH FLAG_0",
                    "PUSH FLAG_1",

                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "PUSH_0",

                    "IFCH_0",
                    "IFCH_1",
                    "B4_0",
                    "IRET_0",
                    "IRET_1",
                    "IRET_2",
                    "IRET_3",
                    "IRET_4",
                    "IRET_5",
                },
                () =>
                { 
                    Assert.AreEqual(UserCodeStartAddress + 0x10, cpu.Registers[REGISTERS.PC]); 
                    Assert.AreEqual(CpuTestsUtils.stackPointer, cpu.Registers[REGISTERS.SP]); 
                    Assert.AreEqual(0x8, cpu.Registers[REGISTERS.FLAGS]); 
                });
        }
    }
}
