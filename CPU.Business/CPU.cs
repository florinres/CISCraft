using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

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
        enum REGISTERS
        {
            FLAGS,
            RG,
            SP,
            T,
            PC,
            IVR,
            ADR,
            MDR,
            R0,
            R1,
            R2,
            R3,
            R4,
            R5,
            R6,
            R7,
            R8,
            R9,
            R10,
            R11,
            R12,
            R13,
            R14,
            R15,
            NEG,
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
		public short[] Registers = new short[MAX_NUM_REG];
		public short SBUS, DBUS, RBUS;
		private ControlUnit _controlUnit;
        public bool ACLOW, INT, CIL;
		public CPU()
		{
			_controlUnit = new ControlUnit();
            _controlUnit.SbusEvent   += OnSbusEvent;
            _controlUnit.DbusEvent   += OnDbusEvent;
            _controlUnit.AluEvent    += OnAluEvent;
            _controlUnit.RbusEvent   += OnRbusEvent;
            _controlUnit.MemoryEvent += OnMemoryEvent;
            _controlUnit.OtherEvent  += OnOtherEvent;
        }
        public string StepMicrocode()
		{
            return _controlUnit.StepMicrocode(ACLOW, Registers[(int)REGISTERS.FLAGS]);
        }
        private void OnSbusEvent(int index)
        {
            SBUS = Registers[index];
            if (index == 25)
                SBUS = (short)~SBUS;

        }
        private void OnDbusEvent(int index)
        {
            DBUS = Registers[index];
            if (index == 25)
               DBUS = (short)~DBUS;
        }

        // This event is the actual alu, I won't create another wrapper method,
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
                        short carryIn = (short)(Registers[(int)REGISTERS.FLAGS] & 0x3);
                        short carryOut = 0;
                        (RBUS, carryOut) = RotateLeftWithCarry(RBUS, carryIn);
                        Registers[(int)REGISTERS.FLAGS] |= carryOut;
                    }
                    break;
                case ALU_OP.RRC:
                    {
                        short carryIn = (short)(Registers[(int)REGISTERS.FLAGS] & 0x3);
                        short carryOut = 0;
                        (RBUS, carryOut) = RotateRightWithCarry(RBUS, carryIn);
                        Registers[(int)REGISTERS.FLAGS] |= carryOut;
                    }
                    break;
            }
        }
        private void OnRbusEvent(int obj)
        {
            for (int i = 1; i < MAX_NUM_REG; i++)
            {
                Registers[i] = RBUS;
                if (i == 25)
                    Registers[i] = (short)~Registers[i];
            }
        }
        private void OnMemoryEvent(int index)
        {
            switch (index)
            {
                case 0 /* None */:
                    break;
                case 1 /* IFCH */:
                    //  _controlUnit.IR = RAM.memory[Registers[(int)REGISTERS.ADR]];
                    break;
                case 2 /* READ */:
                    //  Registers[(int)REGISTERS.MDR] = RAM.memory[Registers[(int)REGISTERS.ADR]];
                    break;
                case 3 /* WRITE */:
                    //  RAM.memory[Registers[(int)REGISTERS.ADR]] = Registers[(int)REGISTERS.MDR];
                    break;
            }
        }
        private void OnOtherEvent(int index)
        {
            switch ((OTHER_EVENTS)index)
            {
                case OTHER_EVENTS.SP_PLUS_2:
                    Registers[(int)REGISTERS.SP] +=2;
                    break;
                case OTHER_EVENTS.SP_MINUS_2:
                    Registers[(int)REGISTERS.SP] -=2;
                    break;
                case OTHER_EVENTS.PC_PLUS_2:
                    Registers[(int)REGISTERS.PC] +=2;
                    break;
                case OTHER_EVENTS.A1BE0:
                    ACLOW = true;
                    break;
                case OTHER_EVENTS.A1BE1:
                    CIL = true;
                    break;
                case OTHER_EVENTS.PdCondA:
                    Registers[(int)REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.CinPdCondA:
                    Registers[(int)REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.PdCondL:
                    Registers[(int)REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.A1BVI:
                    Registers[(int)REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.A0BVI:
                    Registers[(int)REGISTERS.FLAGS] = RBUS; // INCORECT PROBABLY
                    break;
                case OTHER_EVENTS.INTA_SP_MINUS_2:
                    // INTA = 1;
                    Registers[(int)REGISTERS.SP] -= 2;
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

