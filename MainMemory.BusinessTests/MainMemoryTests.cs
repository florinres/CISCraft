using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainMemory.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMemory.Business.Tests
{
    [TestClass()]
    public class MainMemoryTests
    {
        MainMemory? dummyRAM;

        [TestInitialize]
        public void Setup()
        {
            this.dummyRAM = MainMemory.GetMainMemoryInstance();
        }

        [TestMethod()]
        public void SetInternalMachineCodeTest()
        {
            int machineCodeSize = this.dummyRAM.GetMemorySize() + 1;
            byte[] machineCode = new byte[machineCodeSize];
            Random rnd = new();

            for (int i = 0; i < machineCode.Length; i++)
                machineCode[i] = (byte)rnd.Next(0, 2);

            Assert.ThrowsException<InvalidOperationException>(() => this.dummyRAM.SetInternalMachineCode(machineCode));
        }

        [TestMethod()]
        public void SetInternalStackSizeTest()
        {
            Assert.ThrowsException<InvalidOperationException>(() => this.dummyRAM.SetInternalStackSize(this.dummyRAM.GetMemorySize()));

        }

        [TestMethod()]
        public void GetMainMemoryInstanceTest()
        {
            MainMemory secondaryMemory = MainMemory.GetMainMemoryInstance();

            Assert.AreSame(this.dummyRAM, secondaryMemory);
        }
    }
}