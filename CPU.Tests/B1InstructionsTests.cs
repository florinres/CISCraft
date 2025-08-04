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
    [TestClass]
    public class B1InstructionsTests
    {
        CpuTestsUtils? cpuTestsUtils;
        ASM? assembler;
        MemWrapper? memWrapper;
        Ram? ram;
        RegisterWrapper? list;
        Cpu? cpu;
        [TestInitialize]
        public void Setup()
        {
            cpuTestsUtils = new CpuTestsUtils();
            assembler = new ASM();
            memWrapper = new MemWrapper();
            ram = new Ram(memWrapper);
            list = new RegisterWrapper(20);
            cpu = new Cpu(ram,list);

            string jsonPath = Path.GetFullPath(AppContext.BaseDirectory + "../../../../Configs/MPM.json");
            string jsonString = File.ReadAllText(jsonPath);
            cpu.LoadJsonMpm(jsonString);
        }
        [TestMethod()]
        public void Mov_AD_AM_Test()
        {
            string sourceCode = "mov r0, 1";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            Console.Write("Expected Path: ");
            foreach (var label in expectedInstructionPath)
                Console.Write(label + " ");
            Console.WriteLine();

            Console.Write("Real Path: ");
            foreach (var label in realInstructionPath)
                Console.Write(label + " ");
            Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(1,cpu.Registers[GPR.R0]);
            Assert.IsTrue(realInstructionPath.SequenceEqual(expectedInstructionPath));
        }

        // Precondition: Mov_AD_AM_Test
        public void Mov_AD_AD_Test()
        {
            string sourceCode = "mov r1, r0";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(cpu.Registers[GPR.R0],cpu.Registers[GPR.R1]);
            // Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AD_AI_Test()
        {
            string sourceCode = "mov r2, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AD_AX_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AI_AM_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AI_AI_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AI_AX_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AX_AM_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AX_AD_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AX_AI_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
        public void Mov_AX_AX_Test()
        {
            string sourceCode = "mov r1, [r1]";
            int len;
            List<string> realInstructionPath;
            List<string> expectedInstructionPath =
            [
                "IFCH_0",
                "IFCH_1",
                "B1_0",
                "FOS_AM_0",
                "FOSEND_0",
                "FOD_AD_B1_0",
                "MOV_0",
                "WRD_1"
            ];

            ram.LoadMachineCode(assembler.Assemble(sourceCode, out len));

            realInstructionPath = cpuTestsUtils.GetInstructionPath(cpu);

            // Console.Write("Expected Path: ");
            // foreach (var label in expectedInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            // Console.Write("Real Path: ");
            // foreach (var label in realInstructionPath)
            //     Console.Write(label + " ");
            // Console.WriteLine();

            cpuTestsUtils.GenerateTraceLog();

            Assert.AreEqual(2,cpu.Registers[GPR.R1]);
            Assert.AreEqual(expectedInstructionPath, realInstructionPath);
        }
    }
}
