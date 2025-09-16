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
    public class B3InstructionsTests
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
            File.Delete(AppContext.BaseDirectory + "../../../SnapShots_B3.txt");
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

            CpuTestsUtils.GenerateTraceLog(initRamDump, ram.GetMemoryDump(), registerSnapshots, expectedInstructionPath, realInstructionPath, testName, sourceCode, "SnapShots_B3.txt");

            postAssert();
        }
        [TestMethod]
        public void BEQ_Test1()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "BEQ_Test1",
                "beq 10",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BEQ_0",
                    "BEQ_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 4, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BEQ_Test2()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "BEQ_Test2",
                "cmp r0,1\nbeq 6",
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
                    "B3_0",
                    "BEQ_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xE, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BNE_Test1()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "BNE_Test1",
                "cmp r0,1\nbne 10",
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
                    "B3_0",
                    "BNE_0",
                    "BNE_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x8, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BNE_Test2()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "BNE_Test2",
                "bne 10",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BNE_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0"
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xE, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BMI_Test1()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "BMI_Test1",
                "bmi 10",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BMI_0",
                    "BMI_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x4, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BMI_Test2()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Bmi_Test2",
                "cmp r0,2\nbmi 6",
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
                    "B3_0",
                    "BMI_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xE, cpu.Registers[REGISTERS.PC]);
                });

        }
        [TestMethod]
        public void BPL_Test1()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1;
            RunInstructionTest(
                "Bpl_Test2",
                "cmp r0,2\nbpl 6",
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
                    "B3_0",
                    "BPL_0",
                    "BPL_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x8, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BPL_Test2()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Bpl_Test2",
                "bpl 6",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BPL_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xA, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BCS_Test1()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Bcs_Test1",
                "bcs 6",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BCS_0",
                    "BCS_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x4, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BCS_Test2()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = -2;
            RunInstructionTest(
                "Bcs_Test2",
                "cmp r0, 1\nbcs 6",
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
                    "B3_0",
                    "BCS_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xE, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BCC_Test1()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = -2;
            RunInstructionTest(
                "Bcc_Test2",
                "cmp r0, 1\nbcc 6",
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
                    "B3_0",
                    "BCC_0",
                    "BCC_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x8, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BCC_Test2()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Bcc_Test2",
                "bcc 6",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BCC_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xA, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BVS_Test1()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Bvs_Test1",
                "bvs 6",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BVS_0",
                    "BVS_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x4, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BVS_Test2()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1<<15 - 1;
            RunInstructionTest(
                "Bvs_Test2",
                "cmp r0, -20000\nbvs 6",
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
                    "B3_0",
                    "BVS_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xE, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BVC_Test1()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = 1<<15 - 1;
            RunInstructionTest(
                "Bvc_Test2",
                "cmp r0, -20000\nbvc 6",
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
                    "B3_0",
                    "BVC_0",
                    "BVC_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x8, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void BVC_Test2()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Bvc_Test2",
                "bvc 6",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "BVC_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xA, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void JMP_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Jmp_AM_Test",
                "jmp 6",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "JMP_0",
                    "JMP_AM_0",
                    "JMP_AM_2_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0xA, cpu.Registers[REGISTERS.PC]);
                });
        }
        // [TestMethod]
        // public void JMP_AD_Test()
        // {
        //     if (cpu == null) return;

        //     cpu.Registers[GPR.R0] = 6;
        //     RunInstructionTest(
        //         "Jmp_AD_Test",
        //         "jmp r0",
        //         new List<string>
        //         {
        //             "IFCH_0",
        //             "IFCH_1",
        //             "B3_0",
        //             "JMP_0",
        //             "JMP_AD_0",
        //         },
        //         () =>
        //         {
        //             Assert.AreEqual(0xA, cpu.Registers[REGISTERS.PC]);
        //         });
        // }
        [TestMethod]
        public void JMP_AI_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = UserCodeStartAddress + 6;
            RunInstructionTest(
                "Jmp_AI_Test",
                "jmp [r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "JMP_0",
                    "JMP_AI_0",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x6, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void JMP_AX_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = UserCodeStartAddress;
            RunInstructionTest(
                "Jmp_AI_Test",
                "jmp 6[r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "JMP_0",
                    "JMP_AX_0",
                    "JMP_AX_1",
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x6, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void CALL_AM_Test()
        {
            if (cpu == null) return;

            RunInstructionTest(
                "Call_AM_Test",
                "call 2",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "CALL_0",
                    "CALL_AM_0",
                    "CALL_2_0",
                    "CALL_3_0",
                    "CALL_3_1",
                    "CALL_3_2"
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x6, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void CALL_AI_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = UserCodeStartAddress + 6;
            RunInstructionTest(
                "Call_AI_Test",
                "call [r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "CALL_0",
                    "CALL_AI_0",
                    "CALL_3_0",
                    "CALL_3_1",
                    "CALL_3_2"
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x6, cpu.Registers[REGISTERS.PC]);
                });
        }
        [TestMethod]
        public void CALL_AX_Test()
        {
            if (cpu == null) return;

            cpu.Registers[GPR.R0] = UserCodeStartAddress;
            RunInstructionTest(
                "Call_AX_Test",
                "call 6[r0]",
                new List<string>
                {
                    "IFCH_0",
                    "IFCH_1",
                    "B3_0",
                    "CALL_0",
                    "CALL_AX_0",
                    "CALL_AX_1",
                    "CALL_3_0",
                    "CALL_3_1",
                    "CALL_3_2"
                },
                () =>
                {
                    Assert.AreEqual(UserCodeStartAddress + 0x6, cpu.Registers[REGISTERS.PC]);
                });
        }
    }
}
