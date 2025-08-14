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
        enum ALU_OP {
            NONE,
            SBUS,
            DBUS,
            ADD,
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
        struct ALU_FLAGS
        {
            public int CarryFlag;
            public int ZeroFlag;
            public int SignFlag;
            public int OverflowFlag;
        }
        ALU_FLAGS _aluFlags;

        public RegisterWrapper Registers;
        public short SBUS, DBUS, RBUS;
        private short Cin = 0;
        private bool BPO; //Bistabil Pornire/Oprire
        private int previousMIRIndexState, previousMARState;
        private ControlUnit _controlUnit;
        private IMainMemory _mainMemory;
        public bool ACLOW, INT, CIL;
        private OrderedDictionary<string, string[][]> _microProgram;
        public string currentLabel;
        private ushort overflowShift  = 0;
        private ushort signShift      = 1;
        private ushort zeroShift      = 2;
        private ushort carryShift     = 3;
        private ushort interruptShift = 7;
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
            Registers = registers;
            Registers[REGISTERS.ONES] = -1;
            BPO = true; //Enable CPU clock
            previousMARState = 0;
            previousMIRIndexState = 0;
        }
        public (int MAR, int MirIndex) StepMicrocommand()
        {
            if (BPO)
                (previousMARState, previousMIRIndexState) = _controlUnit.StepMicrocommand(ACLOW, Registers[REGISTERS.FLAGS]);
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
            byte[] microcommandsBuffer = new byte[1200];
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
            _mainMemory.ClearMemory();
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
                    SBUS = (byte)Registers[REGISTERS.IRLSB];
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
                    DBUS = (byte)~Registers[REGISTERS.IRLSB];
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
                case ALU_OP.ADD:
                    RBUS = (short)(SBUS + DBUS + Cin);
                    Cin = 0;
                    break;
                // case ALU_OP.SUB:
                //     RBUS = (short)(SBUS - DBUS);
                    break;
                case ALU_OP.AND:
                    RBUS = (short)(SBUS & DBUS);
                    break;
                case ALU_OP.OR:
                    RBUS = (short)(SBUS | DBUS);
                    break;
                case ALU_OP.XOR:
                    RBUS = (short)(SBUS ^ DBUS);
                    break;
                case ALU_OP.ASL:
                    RBUS <<= 1;
                    break;
                case ALU_OP.ASR:
                case ALU_OP.LSR:
                    RBUS >>= 1;
                    break;
                case ALU_OP.ROL:
                    RBUS = (short)BitOperations.RotateLeft((uint)RBUS, 1);
                    break;
                case ALU_OP.ROR:
                    RBUS = (short)BitOperations.RotateRight((uint)RBUS, 1);
                    break;
                case ALU_OP.RLC:
                    {
                        short carryIn = (short)(Registers[REGISTERS.FLAGS] & 0x3);
                        short carryOut = 0;
                        (RBUS, carryOut) = RotateLeftWithCarry(RBUS, carryIn);
                        Registers[REGISTERS.FLAGS] |= carryOut;
                    }
                    break;
                case ALU_OP.RRC:
                    {
                        short carryIn = (short)(Registers[REGISTERS.FLAGS] & 0x3);
                        short carryOut = 0;
                        (RBUS, carryOut) = RotateRightWithCarry(RBUS, carryIn);
                        Registers[REGISTERS.FLAGS] |= carryOut;
                    }
                    break;
            }

            ComputeFlags();
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
                    _controlUnit.IR = _mainMemory.FetchWord(Registers[REGISTERS.ADR]);
                    Registers[REGISTERS.IR] = _controlUnit.IR; // For updateing the UI
                    break;
                case 2 /* READ */:
                    Registers[REGISTERS.MDR] = _mainMemory.FetchWord(Registers[REGISTERS.ADR]);
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
                    ACLOW = true;
                    break;
                case OTHER_EVENTS.A1BE1:
                    CIL = true;
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
                    Registers[REGISTERS.FLAGS] = (short)(Registers[REGISTERS.FLAGS] & (1 << interruptBit));
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
        private (short result, short carryOut) RotateLeftWithCarry(short value, short carryIn)
        {
            short newCarry = 0;
            short shiftedValue = (short)(value << 1);

            if ((short)(value & 0x8000) != 0) newCarry = 0x3;
            if (carryIn == 0x3) shiftedValue |= 1;
            return (shiftedValue, newCarry);
        }
        private (short result, short carryOut) RotateRightWithCarry(short value, short carryIn)
        {
            short newCarry = 0;
            ushort shiftedValue = (ushort)(value >> 1);

            if ((short)(value & 0x1) != 0) newCarry = 0x3;
            if (carryIn == 0x3) shiftedValue |= 0x8000;
            return ((short)shiftedValue, newCarry);
        }
        private (int, int) MarToMpmIndex(int MAR)
        {
            int idx = 0;
            int i = 0, j = 0;
            foreach (var label in _microProgram)
            {
                j=0;
                foreach (var v in label.Value)
                {
                    idx++;
                    if (idx == MAR) return (i,j);
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
    }
}

