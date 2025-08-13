using System.Security.Cryptography;
using Cpu = CPU.Business.CPU;
using Ram = MainMemory.Business.MainMemory;
using MemWrapper = MainMemory.Business.Models.MemoryContentWrapper;
using ASM = Assembler.Business.Assembler;
using CPU.Business.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace CPU.Tests
{
    [TestClass]
    public class B2InstructionsTests
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
            File.Delete(AppContext.BaseDirectory + "../../../SnapShots_B2.txt");
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

            CpuTestsUtils.GenerateTraceLog(initRamDump, ram.GetMemoryDump(), registerSnapshots, expectedInstructionPath, realInstructionPath, testName, sourceCode, "SnapShots_B2.txt");

            postAssert();
        }
        [TestMethod]
        public void Clr_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 2;
            RunInstructionTest(
                "Clr_AD_Test",
                "clr r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "CLR_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(0, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Clr_AI_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Clr_AI_Test",
                "clr [r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AI_B2_0",
                    "CLR_0",
                    "WRD_2"
                },
                () =>
                {
                    Assert.AreEqual(0, ram.FetchWord(0));
                });
        }
        [TestMethod]
        public void Clr_AX_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Clr_AX_Test",
                "clr 2[r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AX_B2_0",
                    "FOD_AX_B2_1",
                    "CLR_0",
                    "WRD_3"
                },
                () =>
                {
                    Assert.AreEqual(0, ram.FetchWord(2));
                });
        }
        [TestMethod]
        public void Neg_AD_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Neg_AD_Test",
                "neg r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "NEG_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(-1, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Inc_AD_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Inc_AD_Test",
                "inc r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "INC_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(1, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Dec_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Dec_AD_Test",
                "dec r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "DEC_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(0, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Asl_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = -1;
            RunInstructionTest(
                "Asl_AD_Test",
                "asl r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "ASL_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(-2, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Asr_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = -4;
            RunInstructionTest(
                "Asr_AD_Test",
                "asr r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "ASR_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(-2, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Lsr_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = -1;
            RunInstructionTest(
                "Lsr_AD_Test",
                "lsr r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "LSR_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(0x7FFF, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Rol_AD_Test()
        {
            if (cpu == null) return;
            cpu.Registers[GPR.R0] = -0x8000;
            RunInstructionTest(
                "Rol_AD_Test",
                "rol r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "ROL_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(0x1, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Ror_AD_Test()
        {
            if (cpu == null) return;
            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Ror_AD_Test",
                "ror r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "ROR_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(-0x8000, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Rlc_AD_Test()
        {
            if (cpu == null) return;
            cpu.Registers[GPR.R0] = -0x8000;
            RunInstructionTest(
                "Rlc_AD_Test",
                "rlc r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "RLC_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(1, cpu.GetCarryFlag());
                });
        }
        [TestMethod]
        public void Rrc_AD_Test()
        {
            if (cpu == null) return;
            cpu.Registers[GPR.R0] = 0x1;
            RunInstructionTest(
                "Rrc_AD_Test",
                "rrc r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "RRC_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(1, cpu.GetCarryFlag());
                });
        }
        [TestMethod]
        public void Push_AD_Test()
        {
            if (cpu == null) return;
            cpu.Registers[GPR.R0] = 0x1;
            RunInstructionTest(
                "Push_AD_Test",
                "push r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "PUSH_0",
                },
                () =>
                {
                    Assert.AreEqual(1, ram.FetchWord(0x6));
                });
        }
        [TestMethod]
        public void Pop_AD_Test()
        {
            if (cpu == null) return;
            cpu.Registers[GPR.R0] = 0x1;
            RunInstructionTest(
                "Pop_AD_Test",
                "push r0\npop r1",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "PUSH_0",
                    "IFCH_0",
                    "IFCH_1",
                    "B2_0",
                    "FOD_AD_B2_0",
                    "POP_0",
                    "POP_1",
                    "POP_2",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(1, cpu.Registers[GPR.R1]);
                    Assert.AreEqual(6, cpu.Registers[REGISTERS.SP]);
                });
        }
    }
}
