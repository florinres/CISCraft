using System;
using System.Diagnostics;
using CPU.Business.Models;

namespace CPU.Business
{
	public class ControlUnit(RegisterWrapper registersWrapper)
    {
        public event Action<int>? SbusEvent;
        public event Action<int>? DbusEvent;
        public event Action<int>? AluEvent;
        public event Action<int>? RbusEvent;
        public event Action<int>? MemoryEvent;
        public event Action<int>? OtherEvent;

        private readonly RegisterWrapper _registersWrapper = registersWrapper;

        public byte PrevMar = 0;
        /// <summary>
        /// Micro Program Memory
        /// ROM memory that holds microinstructions of 36 bits wide.
        /// Key: micro-address
        /// Value: micro-commands
        /// </summary>
        public long[] MPM = new long[150];
        public short IR = 0;
        private int state = 0;
        private int _mirIndex = 0;
        private int _globalIRQState = 0;
        private bool _cilState = false;
        public const short IrDrMask        = 0xF;
        public const short IrSrMask        = 0x3C0;
        public const long SbusMask        = 0xF00000000;
        public const long DbusMask        = 0xF0000000;
        public const long AluMask         = 0xF000000;
        public const long RbusMask        = 0xF00000;
        public const long MemOpMask       = 0xC0000;
        public const long OthersMask      = 0x3C000;
        public const long SuccesorMask    = 0x3800;
        public const long IndexMask       = 0x700;
        public const long TnegFMask       = 0x80;
        public const long AddresMask      = 0x7F;
        public const byte IrDrShift       = 0;
        public const byte IrSrShift       = 6;
        public const byte SbusShift       = 32;
        public const byte DbusShift       = 28;
        public const byte AluShift        = 24;
        public const byte RbusShift       = 20;
        public const byte MemOpShift      = 18;
        public const byte OthersShift     = 14;
        public const byte SuccesorShift   = 11;
        public const byte IndexShift      = 8;
        public const byte TnedFShift      = 7;
        public const byte AddresShift     = 0;
        static public Dictionary<string, int> _microcommandsIndexes = new Dictionary<string, int>
        {
            {"NONE",        0 },

			//SBUS
			{"PdFLAGs",     1 },
            {"PdRGs",       2 },
            {"PdSPs",       3 },
            {"PdTs",        4 },
            {"PdPCs",       5 },
            {"PdIVRs",      6 },
            {"PdADRs",      7 },
            {"PdMDRs",      8 },
            {"PdIR[7...0]s",9 },
            {"PdTsNeg",     10 },
            {"Pd0s",        11 },
            {"Pd-1s",       12 },

			//DBUS
			{"PdFLAGSd",    1 },
            {"PdRGd",       2 },
            {"PdSPd",       3 },
            {"PdTd",        4 },
            {"PdPCd",       5 },
            {"PdIVRd",      6 },
            {"PdADRd",      7 },
            {"PdMDRd",      8 },
            {"PdIR[7...0]d",9 },
            {"PdMDRdNeg",   10 },
            {"Pd0d",        11 },
            {"Pd-1d",       12 },

			//ALU
			{"SBUS",        1 },
            {"DBUS",        2 },
            {"SUM",         3 },
            {"SUB",         4 },
            {"AND",         5 },
            {"OR",          6 },
            {"XOR",         7 },
            {"ASL",         8 },
            {"ASR",         9 },
            {"LSR",         10 },
            {"ROL",         11 },
            {"ROR",         12 },
            {"RLC",         13 },
            {"RRC",         14 },

			//RBUS
			{"PmFLAG",      1 },
            {"PmRG",        2 },
            {"PmSP",        3 },
            {"PmT",         4 },
            {"PmPC",        5 },
            {"PmIVR",       6 },
            {"PmADR",       7 },
            {"PmMDR",       8 },
            {"PmFlag0",     9 },
            {"PmFlag1",     10 },
            {"PmFlag2",     11 },
            {"PmFlag3",     12 },

			// MemOp
			{"IFCH",        1 },
            {"READ",        2 },
            {"WRITE",       3 },

			// OtherOp
            {"+2SP",            1 },
            {"-2SP",            2 },
            {"+2PC",            3 },
            {"A(1)BE0",         4 },
            {"A(1)BE1",         5 },
            {"PdCONDaritm",     6 },
            {"Cin,PdCONDaritm", 7 },
            {"PdCONDlog",       8 },
            {"A(1)BVI",         9 },
            {"A(0)BVI",         10 },
            {"A(0)BPO",         11 },
            {"INTA,-2SP",       12 },
            {"A(0)BE,A(0)BI",   13 },

            // SUCCESOR
            {"STEP",            0 },
            {"JUMPI",           1 },
            {"IF ACLOW JUMPI",  2 },
            {"IF CIL JUMPI",    3 },
            {"IF C JUMPI",      4 },
            {"IF Z JUMPI",      5 },
            {"IF S JUMPI",      6 },
            {"IF V JUMPI",      7 },

            // INDEX
            {"INDEX0",          0 },
            {"INDEX1",          1 },
            {"INDEX2",          2 },
            {"INDEX3",          3 },
            {"INDEX4",          4 },
            {"INDEX5",          5 },
            {"INDEX6",          6 },
            {"INDEX7",          7 },

            // Tneg/F
            {"T",               0 },
            {"F",               1 },
        };

        /// <summary>
        /// Sets or resets the state of the global interrupt flag
        /// which is used to enter into interrupt phase.
        /// </summary>
        /// <param name="irqReq"></param>
        public void SetGlobalIRQState(bool irqReq)
        {
            _globalIRQState = Convert.ToInt32(irqReq);
        }

        /// <summary>
        /// Sets or resets the state of the illegal instruction (CIL)
        /// flag which leads to Exception1 i.e. halt.
        /// </summary>
        /// <param name="cil"></param>
        public void SetCILState(bool cil)
        {
            _cilState = cil;
        }

        /// <summary>
        /// Implements logic for stepping through
        /// the Sequencer, thus going through
        /// instruction phases and sending
        /// necessary command signals.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="ACLOWSignal"></param>
        /// <param name="flagsRegister"></param>
        /// <returns>
        /// The name of the microcode it shall be executed.
        /// This shall be used by UI.
        /// </returns>
		internal (int MAR,int MirIndex) StepMicrocommand(bool ACLOWSignal, short flagsRegister)
        {
            if (_mirIndex != 0)
            {
                return DecodeAndSendCommand();
            }

            do
            {
                switch (state)
                {
                    case 0:
                        PrevMar = _registersWrapper.MAR;
                        _registersWrapper.MIR = MPM[_registersWrapper.MAR];
                        state = 1;
                        break;
                    case 1:
                        bool g_function = this.ComputeConditionG(ACLOWSignal, flagsRegister);
                        if (g_function)
                            _registersWrapper.MAR = (byte)(getMirAddresField() + this.ComputeMARIndex());
                        else _registersWrapper.MAR++;

                        int mirALUBits = getMirAluField();
                        bool aluBIT24 = Convert.ToBoolean(mirALUBits & 1);
                        bool aluBIT25 = Convert.ToBoolean(mirALUBits & (1 << 1));
                        if (!aluBIT24 & !aluBIT25)
                            state = 2;
                        else state = 0;
                        return DecodeAndSendCommand();
                    case 2:
                        state = 3;
                        // Note: this state would normally be reserved for DMA logic
                        // but in the current implementation it shall be ignored.
                        break;
                    case 3:
                        state = 0;
                        break;

                }
            } while (state != 2);

            throw new Exception("Fatal Error: Unknown command");
        }
        internal byte GetSourceRegister()
        {
            return (byte)((IR & IrSrMask) >> IrSrShift);
        }
        internal byte GetDestinationRegister()
        {
            return (byte)((IR & IrDrMask) >> IrDrShift);
        }
        internal void Reset()
        {
            _registersWrapper.MAR = 0;
            _mirIndex = 0;
            state = 0;
            IR = 0;
            PrevMar = 0;
        }
        /// <summary>
        /// Computes the G function which is used to
        /// determine what address to load into MAR.
        /// </summary>
        /// <param name="ACLOWSignal"></param>
        /// <param name="flagsRegister"></param>
        /// <returns></returns>
		private bool ComputeConditionG(bool ACLOWSignal,short flagsRegister)
		{
            bool g_flag = true;
            bool CILFlag = _cilState;
            bool carryFlag = Convert.ToBoolean(flagsRegister & (1<<3));
            bool zeroFlag = Convert.ToBoolean(flagsRegister & (1 << 2));
            bool signFLag = Convert.ToBoolean(flagsRegister & (1 << 1));
            bool overflowFlag = Convert.ToBoolean(flagsRegister & (1 << 0));
            int selectValue = getMirSuccesorField();


            bool bTNegF = Convert.ToBoolean(getMirTNegFField());

            switch (selectValue)
            {
                case 0:
                    g_flag = false; //g <- MIR[7] xor MIR[7]
                    break;
                case 1:
                    g_flag = true; // g<- Not(MIR[7]) xor MIR[7]
                    break;
                case 2:
                    g_flag = (bool)(ACLOWSignal ^ bTNegF);
                    break;
                case 3:
                    g_flag = CILFlag ^ bTNegF;
                    break;
                case 4:
                    g_flag = carryFlag ^ bTNegF;
                    break;
                case 5:
                    g_flag = zeroFlag ^ bTNegF;
                    break;
                case 6:
                    g_flag = signFLag ^ bTNegF;
                    break;
                case 7:
                    g_flag = overflowFlag ^ bTNegF;
                    break;
            }
            return g_flag;
		}
		/// <summary>
        /// Computes the index used for MAR. This would corespond
        /// to the hardware Index Selection Block.
        /// </summary>
        /// <returns> 7 bit address</returns>
		private byte ComputeMARIndex()
		{
            byte marIndex = 0;
            int selectValue = getMirIndexField();
            // Note: since we the hardware behaviour of MIR register has been
            // abstracted, the hardware MIR[10:8] bits corespond to
            // software MIR[7] value.

            int instructionClass1 = getBit(IR, 15) & getBit(IR, 14);
            //coresponds to hardware CL0
            int instructionClass0 = getBit(IR, 15) & getBit(IR, 13);
            //coresponds to hardware CL1

            switch (selectValue)
            {
                case 0:
                    marIndex = 0;
                    break;
                case 1:
                    marIndex = (byte)((instructionClass1 << 1) | instructionClass0);
                    break;
                case 2:
                    marIndex = (byte)(((this.IR & (1 << 11)) | (this.IR & (1 << 10))) >> 10);
                    break;
                case 3:
                    marIndex = (byte)(((this.IR & (1 << 5)) | (this.IR & (1 << 4))) >> 4);
                    break;
                case 4:
                    marIndex = (byte)((
                        (this.IR & (1 << 14))
                        | (this.IR & (1 << 13))
                        | (this.IR & (1 << 12))
                        ) >> 12);
                    break;
                case 5:
                    marIndex = (byte)((
                        (this.IR & (1 << 11))
                        | (this.IR & (1 << 10))
                        | (this.IR & (1 << 9))
                        | (this.IR & (1 << 8))
                        | (this.IR & (1 << 7))
                        ) >> 7);
                    break;
                case 6:
                    marIndex = (byte)( (
                        (this.IR & (1 << 11))
                        | (this.IR & (1 << 10))
                        | (this.IR & (1 << 9))
                        | (this.IR & (1 << 8))
                        ) >> 7);
                    break;
                case 7:
                    marIndex = (byte)(_globalIRQState << 2);
                    break;

            }
            return marIndex;
		}
		private (int MAR, int MirIndex) DecodeAndSendCommand()
		{
            int index = _mirIndex;
            switch (_mirIndex) {
				case 0:
                    SbusEvent?.Invoke(getMirSbusField());
                    break;
				case 1:
                    DbusEvent?.Invoke(getMirDbusField());
                    break;
				case 2:
                    OtherEvent?.Invoke(getMirOthersField());
                    break;
				case 3:
                    AluEvent?.Invoke(getMirAluField());
                    break;
                case 4:
                    RbusEvent?.Invoke(getMirRbusField());
                    break;
                case 5:
                    MemoryEvent?.Invoke(getMirMemOpField());
                    break;
                case 6:
                    Debug.WriteLine("Next MAR= " + _registersWrapper.MAR);
                    break;
			}
            _mirIndex++;
            _mirIndex %= 7;
            return (PrevMar, index);
        }
        private int getMirSbusField()
        {
            return (int)((_registersWrapper.MIR & SbusMask) >> SbusShift);
        }
        private int getMirDbusField()
        {
            return (int)((_registersWrapper.MIR & DbusMask) >> DbusShift);
        }
        private int getMirAluField()
        {
            return (int)((_registersWrapper.MIR & AluMask) >> AluShift);
        }
        private int getMirRbusField()
        {
            return (int)((_registersWrapper.MIR & RbusMask) >> RbusShift);
        }
        private int getMirMemOpField()
        {
            return (int)((_registersWrapper.MIR & MemOpMask) >> MemOpShift);
        }
        private int getMirOthersField()
        {
            return (int)((_registersWrapper.MIR & OthersMask) >> OthersShift);
        }
        private int getMirSuccesorField()
        {
            return (int)((_registersWrapper.MIR & SuccesorMask) >> SuccesorShift);
        }
        private int getMirIndexField()
        {
            return (int)((_registersWrapper.MIR & IndexMask) >> IndexShift);
        }
        private int getMirTNegFField()
        {
            return (int)((_registersWrapper.MIR & TnegFMask) >> TnedFShift);
        }
        private int getMirAddresField()
        {
            return (int)((_registersWrapper.MIR & AddresMask) >> AddresShift);
        }
        private int getBit(short register, ushort bitIndex=0)
        {
            return (int)((register & (1 << bitIndex)) >> bitIndex);
        }
    }

}