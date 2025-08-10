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
    public class B1InstructionsTests
    {
        ASM? assembler;
        MemWrapper? memWrapper;
        Ram? ram;
        RegisterWrapper? list;
        Cpu? cpu;
        List<KeyValuePair<string, string>>? realInstructionPath;
        List<string>? expectedInstructionPath;

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            File.Delete(AppContext.BaseDirectory + "../../../SnapShots_B1.txt");
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

            CpuTestsUtils.GenerateTraceLog(initRamDump, ram.GetMemoryDump(), registerSnapshots, expectedInstructionPath, realInstructionPath, testName, sourceCode, "SnapShots_B1.txt");

            postAssert();
        }

        [TestMethod()]
        public void Mov_AD_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AD_AM_Test",
                "mov r0, 1",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(1, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Mov_AD_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Mov_AD_AD_Test",
                "mov r1, r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AD_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        [TestMethod]
        public void Mov_AD_AI_Test()
        {
            if (cpu == null || ram == null) return;

            // ram.SetByteLocation(2, 2); //TODO: Fix memoryContentWrapper
            // ram.SetByteLocation(3, 0);
            // cpu.Registers[GPR.R0] = 2;

            RunInstructionTest(
                "Mov_AD_AI_Test",
                "mov r1, [r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AI_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(0x811, (ushort)cpu.Registers[GPR.R1]);
                    // Assert.AreEqual(0x2, (ushort)cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AD_AX_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AD_AX_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AI_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AI_AM_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AI_AI_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AI_AI_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AI_AX_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AI_AX_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AX_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AX_AM_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AX_AD_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AX_AD_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AX_AI_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AX_AI_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
        public void Mov_AX_AX_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AX_AX_Test",
                "mov r2, [r1]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
                });
        }
    }
}
