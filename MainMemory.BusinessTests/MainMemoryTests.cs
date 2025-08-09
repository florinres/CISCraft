using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainMemory.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemWrapper = MainMemory.Business.Models.MemoryContentWrapper;

namespace MainMemory.Business.Tests
{
    [TestClass()]
    public class MainMemoryTests
    {
        MemWrapper? memWrapper;
        MainMemory? dummyRAM;

        [TestInitialize]
        public void Setup()
        {
            this.memWrapper = new MemWrapper();
            this.dummyRAM = new MainMemory(memWrapper);
        }

        [TestMethod()]
        public void SetInternalMachineCodeTest()
        {
            int machineCodeSize = this.dummyRAM.GetMemorySize() + 1;
            byte[] machineCode = new byte[machineCodeSize];
            Random rnd = new();

            for (int i = 0; i < machineCode.Length; i++)
                machineCode[i] = (byte)rnd.Next(0, 2);

            Assert.ThrowsException<InvalidOperationException>(() => this.dummyRAM.LoadMachineCode(machineCode));
        }

        [TestMethod()]
        public void SetInternalStackSizeTest()
        {
            Assert.ThrowsException<InvalidOperationException>(() => this.dummyRAM.SetStackSize(this.dummyRAM.GetMemorySize()));

        }

        [TestMethod()]
        public void SetInternalISRTest()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => this.dummyRAM.SetISR(21, null));

        }

        [TestMethod()]
        public void ClearInternlISRTest()
        {
            ushort interruptReturnOpCode = 0xe00f;
            int interruptSizeMax = 0x1db9;
            ushort isrSegment = 0x2fed;
            byte[] memoryDump = this.dummyRAM.GetMemoryDump();
            bool ok = true;

            for(int i = isrSegment; (i < interruptSizeMax - 1) && ok; i+=2)
            {
                ushort word = (ushort)(memoryDump[i] | (memoryDump[i + 1] << 8));

                if (word != interruptReturnOpCode)
                    ok = false;
            }

            Assert.IsTrue(ok);
        }
    }
}