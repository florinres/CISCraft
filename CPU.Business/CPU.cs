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
        public RegisterWrapper Registers;
		public short SBUS, DBUS, RBUS;
        private ControlUnit _controlUnit;
        private IMainMemory _mainMemory;
        public bool ACLOW, INT, CIL;
		public CPU(IMainMemory mainMemory, RegisterWrapper registers)
        {
            _controlUnit = new ControlUnit();
            _controlUnit.SbusEvent += OnSbusEvent;
            _controlUnit.DbusEvent += OnDbusEvent;
            _controlUnit.AluEvent += OnAluEvent;
            _controlUnit.RbusEvent += OnRbusEvent;
            _controlUnit.MemoryEvent += OnMemoryEvent;
            _controlUnit.OtherEvent += OnOtherEvent;
            _mainMemory = mainMemory;
            Registers = registers;
        }
        public (int MAR, int MirIndex) StepMicrocode()
		{
            return _controlUnit.StepMicrocode(ACLOW, Registers[REGISTERS.FLAGS]);
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
            var deserializedData = JsonSerializer.Deserialize<OrderedDictionary<string, string[][]>>(jsonString);
            var labelsAddresses = new Dictionary<string, byte>();
            byte[] microcommandsBuffer = new byte[1200];
            int bufferIndex = 0;

            if (deserializedData == null) return;

            foreach (var routine in deserializedData)
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
            foreach (var routine in deserializedData)
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

            for (int i = 0; i <= addressCounter*10;)
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
        private void OnSbusEvent(int index)
        {
            switch((REGISTERS)index)
            {
                case REGISTERS.NEG:
                    SBUS = (short)~Registers[REGISTERS.T];
                    break;
                case REGISTERS.RG:
                    int gprIndex = _controlUnit.GetSourceRegister();
                    SBUS = Registers[(GPR)gprIndex];
                    break;
                case REGISTERS.IR:
                    SBUS = (byte)Registers[REGISTERS.IR];
                    break;
                default:
                    SBUS = Registers[(REGISTERS)index];
                    break;
            }

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
                case REGISTERS.IR:
                    DBUS = (byte)~Registers[REGISTERS.IR];
                    break;
                default:
                    DBUS = Registers[(REGISTERS)index];
                    break;
            }
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
                    RBUS = (short)(SBUS + DBUS);
                    break;
                case ALU_OP.SUB:
                    RBUS = (short)(SBUS - DBUS);
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
        }
        private void OnRbusEvent(int index)
        {
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
            switch ((OTHER_EVENTS)index)
            {
                case OTHER_EVENTS.SP_PLUS_2:
                    Registers[REGISTERS.SP] +=2;
                    break;
                case OTHER_EVENTS.SP_MINUS_2:
                    Registers[REGISTERS.SP] -=2;
                    break;
                case OTHER_EVENTS.PC_PLUS_2:
                    Registers[REGISTERS.PC] +=2;
                    break;
                case OTHER_EVENTS.A1BE0:
                    ACLOW = true;
                    break;
                case OTHER_EVENTS.A1BE1:
                    CIL = true;
                    break;
                case OTHER_EVENTS.PdCondA:
                    Registers[REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.CinPdCondA:
                    Registers[REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.PdCondL:
                    Registers[REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.A1BVI:
                    Registers[REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.A0BVI:
                    Registers[REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.INTA_SP_MINUS_2:
                    // INTA = 1;
                    Registers[REGISTERS.SP] -= 2;
                    break;
                case OTHER_EVENTS.A0BE_A0BI:
                    //resetarea bitilor de exceptie;
                    break;
            }
        }
        private (short result,short carryOut) RotateLeftWithCarry(short value, short carryIn)
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
    }
}

