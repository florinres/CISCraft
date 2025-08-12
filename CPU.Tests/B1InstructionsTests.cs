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

        [TestMethod]
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
                    Assert.AreEqual(cpu.Registers[GPR.R0], cpu.Registers[GPR.R1]);
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
        [TestMethod]
        public void Mov_AD_AX_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AD_AX_Test",
                "mov r1, 2[r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AX_0",
                    "FOS_AX_1",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "MOV_0",
                    "WRD_1"
                },
                () =>
                {
                    // Memory contents:
                    // 0: numerial value of the instruction (0xC11)
                    // 2: offset operand of the source register (0x2)
                    // 4 to the end: 0 
                    Assert.AreEqual(2, cpu.Registers[GPR.R1]);
                });
        }
        [TestMethod]
        public void Mov_AI_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AI_AM_Test",
                "mov [r0], 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AI_B1_0",
                    "MOV_0",
                    "WRD_2"
                },
                () =>
                {
                    Assert.AreEqual(2, ram.FetchWord(0));
                });
        }
        [TestMethod]
        public void Mov_AI_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Mov_AI_AD_Test",
                "mov [r1], r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AD_0",
                    "FOD_AI_B1_0",
                    "MOV_0",
                    "WRD_2"
                },
                () =>
                {
                    Assert.AreEqual(1, ram.FetchWord(0));
                });
        }
        [TestMethod]
        public void Mov_AI_AI_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 2;
            RunInstructionTest(
                "Mov_AI_AI_Test",
                "mov [r1], [r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AI_0",
                    "FOSEND_0",
                    "FOD_AI_B1_0",
                    "MOV_0",
                    "WRD_2"
                },
                () =>
                {
                    Assert.AreEqual(0, ram.FetchWord(0));
                });
        }
        [TestMethod]
        public void Mov_AI_AX_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AI_AX_Test",
                "mov [r1], 2[r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AX_0",
                    "FOS_AX_1",
                    "FOSEND_0",
                    "FOD_AI_B1_0",
                    "MOV_0",
                    "WRD_2"
                },
                () =>
                {
                    Assert.AreEqual(2, ram.FetchWord(0));
                });
        }
        [TestMethod]
        public void Mov_AX_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AX_AM_Test",
                "mov 2[r0], 1",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AX_B1_0",
                    "FOD_AX_B1_1",
                    "MOV_0",
                    "WRD_3"
                },
                () =>
                {
                    Assert.AreEqual(1, ram.FetchWord(2));
                });
        }
        [TestMethod]
        public void Mov_AX_AD_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Mov_AX_AD_Test",
                "mov 2[r1], r0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AD_0",
                    "FOD_AX_B1_0",
                    "FOD_AX_B1_1",
                    "MOV_0",
                    "WRD_3"
                },
                () =>
                {
                    Assert.AreEqual(1, ram.FetchWord(2));
                });
        }
        [TestMethod]
        public void Mov_AX_AI_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 4;
            RunInstructionTest(
                "Mov_AX_AI_Test",
                "mov 2[r1], [r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AI_0",
                    "FOSEND_0",
                    "FOD_AX_B1_0",
                    "FOD_AX_B1_1",
                    "MOV_0",
                    "WRD_3"
                },
                () =>
                {
                    Assert.AreEqual(0, ram.FetchWord(2));
                });
        }
        [TestMethod]
        public void Mov_AX_AX_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Mov_AX_AX_Test",
                "mov 2[r1], 6[r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AX_0",
                    "FOS_AX_1",
                    "FOSEND_0",
                    "FOD_AX_B1_0",
                    "FOD_AX_B1_1",
                    "MOV_0",
                    "WRD_3"
                },
                () =>
                {
                    Assert.AreEqual(0, ram.FetchByte(2));
                });
        }
        [TestMethod]
        public void Add_AD_AM_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 2;
            RunInstructionTest(
                "Add_AD_AM_Test",
                "add r0, 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "ADD_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(4, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Sub_AD_AM_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 2;
            RunInstructionTest(
                "Sub_AD_AM_Test",
                "sub r0, 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "SUB_0",
                    "WRD_1"
                },
                () =>
                {
                    Assert.AreEqual(0, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        // Test Signed flag
        public void Cmp_AD_AM_Test1()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Cmp_AD_AM_Test1",
                "cmp r0, 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "CMP_0",
                },
                () =>
                {
                    Assert.AreEqual(0x0002,cpu.Registers[REGISTERS.FLAGS]);
                });
        }
        [TestMethod]
        // Test Zero and Carry flag
        public void Cmp_AD_AM_Test2()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 2;
            RunInstructionTest(
                "Cmp_AD_AM_Test2",
                "cmp r0, 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "CMP_0",
                },
                () =>
                {
                    Assert.AreEqual(0x000C,cpu.Registers[REGISTERS.FLAGS]);
                });
        }
        [TestMethod]
        // Test carry flag
        public void Cmp_AD_AM_Test3()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Cmp_AD_AM_Test3",
                "cmp r0, 0",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "CMP_0",
                },
                () =>
                {
                    Assert.AreEqual(0x0008,cpu.Registers[REGISTERS.FLAGS]);
                });
        }
        // Test overflow flag
        [TestMethod]
        public void Cmp_AD_AM_Test4()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1<<15 - 1;
            RunInstructionTest(
                "Cmp_AD_AM_Test4",
                "cmp r0, -20000",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "CMP_0",
                },
                () =>
                {
                    Assert.AreEqual(0x0003,cpu.Registers[REGISTERS.FLAGS]);
                });
        }
        // Test overflow and carry flag
        [TestMethod]
        public void Cmp_AD_AM_Test5()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = -(1<<15 - 1);
            RunInstructionTest(
                "Cmp_AD_AM_Test5",
                "cmp r0, 20000",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "CMP_0",
                },
                () =>
                {
                    Assert.AreEqual(0x0009,cpu.Registers[REGISTERS.FLAGS]);
                });
        }
        [TestMethod]
        public void And_AD_AM_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 2;
            RunInstructionTest(
                "And_AD_AM_Test",
                "and r0, 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "AND_0",
                    "WRD_1",
                },
                () =>
                {
                    Assert.AreEqual(2, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Or_AD_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Xor_AD_AM_Test",
                "or r0, 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "OR_0",
                    "WRD_1",
                },
                () =>
                {
                    Assert.AreEqual(2, cpu.Registers[GPR.R0]);
                });
        }
        [TestMethod]
        public void Xor_AD_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Xor_AD_AM_Test",
                "xor r0, 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B1_0",
                    "FOS_AM_0",
                    "FOSEND_0",
                    "FOD_AD_B1_0",
                    "XOR_0",
                    "WRD_1",
                },
                () =>
                {
                    Assert.AreEqual(2, cpu.Registers[GPR.R0]);
                });
        }
    }
}
