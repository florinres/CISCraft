using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using CPU.Business.Models;
using MainMemory.Business;

namespace CPU.Business
{
    public class CPU
    {
        const byte MAX_NUM_REG = 23;
        enum ALU_OP
        {
            NONE,
            SBUS,
            DBUS,
            SUM,
            SUB,
            AND,
            OR,
            XOR,
            ASL,
            ASR,
            LSR,
            ROL,
            ROR,
            RLC,
            RRC,
        }

        enum OTHER_EVENTS
        {
            SP_PLUS_2 = 1,
            SP_MINUS_2,
            PC_PLUS_2,
            A1BE0,
            A1BE1,
            PdCondA,
            CinPdCondA,
            PdCondL,
            A1BVI,
            A0BVI,
            A0BPO,
            INTA_SP_MINUS_2,
            A0BE_A0BI,
        }

        public RegisterWrapper Registers;
        public short SBUS, DBUS, RBUS;
        public short Cin = 0;
        private bool BPO; //Bistabil Pornire/Oprire
        private int previousMIRIndexState, previousMARState;
        private ControlUnit _controlUnit;
        private InterruptController _interruptController;

        private IMainMemory _mainMemory;
        private OrderedDictionary<string, string[][]> _microProgram;
        public string currentLabel;
        private ushort overflowShift = 0;
        private ushort signShift = 1;
        private ushort zeroShift = 2;
        private ushort carryShift = 3;
        private ushort interruptShift = 7;
        private bool _globalIRQ = false;
        private bool CinPdCondaritm = false;
        private bool PdCondaritm = false;
        private bool PdCondlogic = false;
        public CPU(IMainMemory mainMemory, RegisterWrapper registers)
        {
            _controlUnit = new ControlUnit(registers);
            _controlUnit.SbusEvent += OnSbusEvent;
            _controlUnit.DbusEvent += OnDbusEvent;
            _controlUnit.AluEvent += OnAluEvent;
            _controlUnit.RbusEvent += OnRbusEvent;
            _controlUnit.MemoryEvent += OnMemoryEvent;
            _controlUnit.OtherEvent += OnOtherEvent;
            _mainMemory = mainMemory;
            _microProgram = new OrderedDictionary<string, string[][]>();
            _interruptController = new InterruptController();
            Registers = registers;
            Registers[REGISTERS.ONES] = -1;
            Registers[REGISTERS.SP] = 0x200;
            BPO = true; //Enable CPU clock
            previousMARState = 0;
            previousMIRIndexState = 0;
        }
        public (int MAR, int MirIndex) StepMicrocommand()
        {
            if (BPO)
            {
                _controlUnit.SetGlobalIRQState(_globalIRQ);
                (previousMARState, previousMIRIndexState) = _controlUnit.StepMicrocommand(Registers[Exceptions.ACLOW], Registers[REGISTERS.FLAGS]);

                bool[] irqs = new bool[]
                {
                    Registers[IRQs.IRQ0],
                    Registers[IRQs.IRQ1],
                    Registers[IRQs.IRQ2],
                    Registers[IRQs.IRQ3]
                };
                bool[] exceptions = new bool[]
                {
                    Registers[Exceptions.ACLOW],
                    Registers[Exceptions.CIL],
                    Registers[Exceptions.Reserved0],
                    Registers[Exceptions.Reserved1]
                };

                Dictionary<string, bool> interruptPriorities = new Dictionary<string, bool>();
                interruptPriorities = _interruptController.CheckInterruptSignals(irqs, exceptions);
                bool interrupAck = Convert.ToBoolean(Registers[REGISTERS.FLAGS] & (1 << interruptShift));

                if (previousMARState == 0 && previousMIRIndexState == 0)
                {
                    // check only when entering the Instruction Fetch phase

                    bool okInstructionCode = CheckInstructionCode();

                    if (okInstructionCode)
                        Registers[Exceptions.CIL] = false;
                    else Registers[Exceptions.CIL] = true;
                }

                if (interrupAck)
                {
                    int i = 0;
                    bool prioritisedInterruptsState = false;
                    foreach (var kvp in interruptPriorities)
                    {
                        if (kvp.Value)
                        {
                            irqs[i] = false;
                        }
                        prioritisedInterruptsState |= kvp.Value;
                        i++;
                    }
                    _globalIRQ = interrupAck & prioritisedInterruptsState;
                }


                if (_globalIRQ)
                    Registers[REGISTERS.IVR] = _interruptController.ComputeInterruptVector(exceptions);
                _controlUnit.SetCILState(Registers[Exceptions.CIL]);

            }
            return (previousMARState, previousMIRIndexState);
        }
        /// <summary>
        /// This will load the json MPM configuration in the actual MPM.
        /// it needs to convert it from an json object into a long array
        /// This method might take some time so we can call it on a separate thread.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void LoadJsonMpm(string jsonString, bool debug = false)
        {
            byte addressCounter = 0;
            int mpmIndex = 0;
            _microProgram = JsonSerializer.Deserialize<OrderedDictionary<string, string[][]>>(jsonString);
            var labelsAddresses = new Dictionary<string, byte>();
            byte[] microcommandsBuffer = new byte[1700];
            int bufferIndex = 0;

            if (_microProgram == null) return;

            foreach (var routine in _microProgram)
            {
                labelsAddresses[routine.Key] = addressCounter;
                foreach (var microinstruction in routine.Value)
                {
                    addressCounter++;
                }
            }

            if (debug)
            {
                foreach (var kvp in labelsAddresses)
                {
                    Console.WriteLine(kvp.Key + " " + kvp.Value);
                }
            }

            addressCounter = 0;
            foreach (var routine in _microProgram)
            {
                var microinstructions = routine.Value;
                if (debug) Console.WriteLine(" " + routine.Key);
                for (int i = 0; i < microinstructions.Length; i++)
                {
                    var microinstruction = microinstructions[i];
                    if (debug) Console.Write(addressCounter + ": ");
                    for (int j = 0; j < microinstruction.Length; j++)
                    {
                        var microcommand = microinstruction[j];

                        if (microcommand == "0")
                        {
                            if (debug) Console.WriteLine(0);
                            microcommandsBuffer[bufferIndex] = 0;
                            bufferIndex++;
                            continue;
                        }

                        if (j == 9)
                        {
                            if (microcommand.Contains("+"))
                            {
                                var label = microcommand.Substring(0, microcommand.IndexOf('+'));
                                byte offset = Byte.Parse(microcommand.Substring(microcommand.IndexOf('+') + 1));
                                if (debug) Console.WriteLine(labelsAddresses[label] + offset);
                                microcommandsBuffer[bufferIndex] = (byte)(labelsAddresses[label] + offset);
                            }
                            else
                            {
                                if (debug) Console.WriteLine(labelsAddresses[microcommand]);
                                microcommandsBuffer[bufferIndex] = labelsAddresses[microcommand];
                            }
                        }
                        else
                        {
                            if (debug) Console.Write(ControlUnit._microcommandsIndexes[microcommand] + " ");
                            microcommandsBuffer[bufferIndex] = (byte)ControlUnit._microcommandsIndexes[microcommand];
                        }
                        bufferIndex++;
                    }
                    addressCounter++;
                }
            }

            for (int i = 0; i <= addressCounter * 10;)
            {
                _controlUnit.MPM[mpmIndex] =
                    ((long)microcommandsBuffer[i++] << ControlUnit.SbusShift)       |
                    ((long)microcommandsBuffer[i++] << ControlUnit.DbusShift)       |
                    ((long)microcommandsBuffer[i++] << ControlUnit.AluShift)        |
                    ((long)microcommandsBuffer[i++] << ControlUnit.RbusShift)       |
                    ((long)microcommandsBuffer[i++] << ControlUnit.MemOpShift)      |
                    ((long)microcommandsBuffer[i++] << ControlUnit.OthersShift)     |
                    ((long)microcommandsBuffer[i++] << ControlUnit.SuccesorShift)   |
                    ((long)microcommandsBuffer[i++] << ControlUnit.IndexShift)      |
                    ((long)microcommandsBuffer[i++] << ControlUnit.TnedFShift)      |
                    ((long)microcommandsBuffer[i++] << ControlUnit.AddresShift);
                mpmIndex++;
            }
        }
        public void ResetProgram()
        {
            for (int i = 0; i <= (int)REGISTERS.IR; i++)
            {
                Registers[(REGISTERS)i] = 0;
            }
            for (int i = 0; i <= (int)GPR.R15; i++)
            {
                Registers[(GPR)i] = 0;
            }
            Registers[REGISTERS.ONES] = -1;
            Registers[REGISTERS.SP] = 0x200;
            _controlUnit.Reset();
            BPO = true; //Reactivate CPU clock
        }
        public (string, string[]) GetCurrentLabel(int MAR)
        {
            string label = "";
            var indexes = MarToMpmIndex(MAR + 1);
            label = _microProgram.ElementAt(indexes.Item1).Key + "_" + indexes.Item2;
            return (label, _microProgram.ElementAt(indexes.Item1).Value[indexes.Item2]);

        }
        private void OnSbusEvent(int index)
        {
            switch ((REGISTERS)index)
            {
                case REGISTERS.NEG:
                    SBUS = (short)~Registers[REGISTERS.T];
                    break;
                case REGISTERS.RG:
                    int gprIndex = _controlUnit.GetSourceRegister();
                    SBUS = Registers[(GPR)gprIndex];
                    break;
                case REGISTERS.IRLSB:
                    SBUS = (short)(Registers[REGISTERS.IR] & 0xFF);
                    break;
                default:
                    SBUS = Registers[(REGISTERS)index];
                    break;
            }
            Debug.WriteLine("SBUS= " + SBUS.ToString());
        }
        private void OnDbusEvent(int index)
        {
            switch ((REGISTERS)index)
            {
                case REGISTERS.NEG:
                    DBUS = (short)~Registers[REGISTERS.MDR];
                    break;
                case REGISTERS.RG:
                    int gprIndex = _controlUnit.GetDestinationRegister();
                    DBUS = Registers[(GPR)gprIndex];
                    break;
                case REGISTERS.IRLSB:
                    DBUS = (short)(Registers[REGISTERS.IR] & 0xFF);
                    break;
                default:
                    DBUS = Registers[(REGISTERS)index];
                    break;
            }
            Debug.WriteLine("DBUS= " + DBUS.ToString());
        }

        // I will just use one and not try to immitate the hardware.
        private void OnAluEvent(int index)
        {
            ALU_OP operation = (ALU_OP)index;

            switch (operation)
            {
                case ALU_OP.NONE:
                    break;
                case ALU_OP.SBUS:
                    RBUS = SBUS;
                    break;
                case ALU_OP.DBUS:
                    RBUS = DBUS;
                    break;
                case ALU_OP.SUM:
                    RBUS = (short)(SBUS + DBUS + Cin);
                    Cin = 0;
                    ComputeFlags();
                    break;
                    // case ALU_OP.SUB:
                    //     RBUS = (short)(SBUS - DBUS);
                case ALU_OP.AND:
                    RBUS = (short)(SBUS & DBUS);
                    ComputeFlags();
                    break;
                case ALU_OP.OR:
                    RBUS = (short)(SBUS | DBUS);
                    ComputeFlags();
                    break;
                case ALU_OP.XOR:
                    RBUS = (short)(SBUS ^ DBUS);
                    ComputeFlags();
                    break;
                case ALU_OP.ASL:
                    RBUS <<= 1;
                    ComputeFlags();
                    break;
                case ALU_OP.ASR:
                    short msbRbus = (short)(RBUS & 0x8000);
                    RBUS >>= 1;
                    RBUS |= msbRbus;
                    ComputeFlags();
                    break;
                case ALU_OP.LSR:
                    RBUS = (short)((ushort)RBUS >> 1);
                    ComputeFlags();
                    break;
                case ALU_OP.ROL:
                    RBUS = (short)RotateLeft((ushort)RBUS, 1);
                    ComputeFlags();
                    break;
                case ALU_OP.ROR:
                    RBUS = (short)RotateRight((ushort)RBUS, 1);
                    ComputeFlags();
                    break;
                case ALU_OP.RLC:
                    RotateLeftWithCarry();
                    break;
                case ALU_OP.RRC:
                    RotateRightWithCarry();
                    break;
            }

            Debug.WriteLine("RBUS= " + RBUS.ToString());
        }

        private void OnRbusEvent(int index)
        {
            if (index == 0)
                return;

            switch ((REGISTERS)index)
            {
                case REGISTERS.NEG:
                    Registers[(REGISTERS)index] = RBUS;
                    break;
                case REGISTERS.RG:
                    int gprIndex = _controlUnit.GetDestinationRegister();
                    Registers[(GPR)gprIndex] = RBUS;
                    break;
                default:
                    Registers[(REGISTERS)index] = RBUS;
                    break;
            }
        }
        private void OnMemoryEvent(int index)
        {
            switch (index)
            {
                case 0 /* None */:
                    break;
                case 1 /* IFCH */:
                    _controlUnit.IR = _mainMemory.FetchWord((ushort)Registers[REGISTERS.ADR]);
                    Registers[REGISTERS.IR] = _controlUnit.IR; // For updateing the UI
                    break;
                case 2 /* READ */:
                    Registers[REGISTERS.MDR] = _mainMemory.FetchWord((ushort)Registers[REGISTERS.ADR]);
                    break;
                case 3 /* WRITE */:
                    _mainMemory.SetWordLocation(Registers[REGISTERS.ADR], Registers[REGISTERS.MDR]);
                    break;
            }
        }
        private void OnOtherEvent(int index)
        {
            int flagsMask0 = ~8;
            int flagsMask1 = ~6;

            int signBit = 1;
            int zeroBit = 2;
            int carryBit = 3;
            int interruptBit = 7; // Check BVI in the documentation

            switch ((OTHER_EVENTS)index)
            {
                case OTHER_EVENTS.SP_PLUS_2:
                    Registers[REGISTERS.SP] += 2;
                    break;
                case OTHER_EVENTS.SP_MINUS_2:
                    Registers[REGISTERS.SP] -= 2;
                    break;
                case OTHER_EVENTS.PC_PLUS_2:
                    Registers[REGISTERS.PC] += 2;
                    break;
                case OTHER_EVENTS.A1BE0:
                    Registers[Exceptions.ACLOW] = true;
                    break;
                case OTHER_EVENTS.A1BE1:
                    Registers[Exceptions.CIL] = true;
                    break;
                case OTHER_EVENTS.PdCondA:
                    PdCondaritm = true;
                    break;
                case OTHER_EVENTS.CinPdCondA:
                    CinPdCondaritm = true;
                    Cin = 1;
                    break;
                case OTHER_EVENTS.PdCondL:
                    PdCondlogic = true;
                    break;
                case OTHER_EVENTS.A1BVI:
                    Registers[REGISTERS.FLAGS] = (short)(Registers[REGISTERS.FLAGS] | (1 << interruptBit));
                    break;
                case OTHER_EVENTS.A0BVI:
                    Registers[REGISTERS.FLAGS] = (short)(Registers[REGISTERS.FLAGS] & ~(1 << interruptBit));
                    break;
                case OTHER_EVENTS.INTA_SP_MINUS_2:
                    // INTA = 1;
                    Registers[REGISTERS.SP] -= 2;
                    break;
                case OTHER_EVENTS.A0BE_A0BI:
                    //resetarea bitilor de exceptie;
                    break;
                case OTHER_EVENTS.A0BPO:
                    //Adu la 0 BPO
                    //Disable CPU clock
                    BPO = false;
                    break;
            }
        }
        private void RotateLeftWithCarry()
        {
            ushort buf = 0;
            ushort oldCarry = GetCarryFlag();

            SetCarryFlag((ushort)((RBUS & -0x8000) >> 16));
            buf = RotateLeft((ushort)RBUS, 1);
            buf &= (ushort)(buf & ~1);
            buf |= oldCarry;

            RBUS = (short)buf;
        }
        private void RotateRightWithCarry()
        {
            ushort buf = 0;
            ushort oldCarry = GetCarryFlag();

            SetCarryFlag((ushort)(RBUS & 0x1));
            buf = RotateRight((ushort)RBUS, 1);
            buf = (ushort)(buf & ~0x8000);
            buf |= (ushort)(oldCarry << 16);

            RBUS = (short)buf;
        }
        private (int, int) MarToMpmIndex(int MAR)
        {
            int idx = 0;
            int i = 0, j = 0;
            foreach (var label in _microProgram)
            {
                j = 0;
                foreach (var v in label.Value)
                {
                    idx++;
                    if (idx == MAR) return (i, j);
                    j++;
                }
                i++;
            }
            return (0, 0);
        }
        // Return the status for Carry, Zero, Sign and Overflow
        private void ComputeArithmeticFlags()
        {
            ComputeLogicFlags();

            if ((ushort)RBUS < Math.Max((ushort)SBUS, (ushort)DBUS))
            {
                Registers[REGISTERS.FLAGS] |= (short)(1 << carryShift);
            }

            if (
                 (((SBUS ^ DBUS) & 0x8000) == 0) &&
                 (((SBUS ^ RBUS) & 0x8000) != 0)
                )
            {
                Registers[REGISTERS.FLAGS] |= (short)(1 << overflowShift);
            }
        }
        // Return the status only for Zero and Sign
        private void ComputeLogicFlags()
        {
            Registers[REGISTERS.FLAGS] = 0;
            if (RBUS == 0)
            {
                Registers[REGISTERS.FLAGS] |= (short)(1 << zeroShift);
            }

            if ((RBUS & 0x8000) == 0x8000)
            {
                Registers[REGISTERS.FLAGS] |= (short)(1 << signShift);
            }
        }
        private void ComputeFlags()
        {
            if (CinPdCondaritm || PdCondaritm)
                ComputeArithmeticFlags();
            if (PdCondlogic)
                ComputeLogicFlags();

            CinPdCondaritm = false;
            PdCondaritm = false;
            PdCondlogic = false;
        }
        private ushort RotateLeft(ushort value, int count)
        {
            const ushort bits = 16;
            count &= bits - 1;
            return (ushort)((value << count) | (value >> (bits - count)));
        }
        private ushort RotateRight(ushort value, int count)
        {
            const ushort bits = 16;
            count &= bits - 1;
            return (ushort)((value >> count) | (value << (bits - count)));
        }
        public ushort GetCarryFlag()
        {
            return (ushort)((Registers[REGISTERS.FLAGS] & (1<<carryShift)) >> carryShift);
        }
        private void SetCarryFlag(ushort value)
        {
            Registers[REGISTERS.FLAGS] |= (short)((value & 1) << carryShift);
        }
        public ushort GetInterruptFlag()
        {
            return (ushort)((Registers[REGISTERS.FLAGS] & (1<<interruptShift)) >> interruptShift);
        }
        /// <summary>
        /// Checks instruction code if it belongs to classes B1 to B4. Returns 'true' if so.
        /// Returns 'false' if illegal code (CIL) is detected.
        /// </summary>
        /// <returns></returns>
        private bool CheckInstructionCode()
        {
            short instructionCode = _controlUnit.IR;

            short b1Bit = (short)(instructionCode & (1 << 15));
            short b2Bit = (short)(instructionCode & (1 << 14));
            short b3Bit = (short)(instructionCode & (1 << 13));
            short b4Bit = (short)(instructionCode & (1 << 12));

            if (b1Bit == 0) // B1 class
                return true;
            else if (b2Bit == 0) //B2 class
                return true;
            else if (b3Bit == 0) // B3 class
                return true;
            else if (b4Bit == 0) // B4 class
                return true;
            else return false; // CIL
        }
    }
}

