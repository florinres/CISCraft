using System;

namespace CPU.Business
{
	public class ControlUnit
	{
        public event Action<int>? SbusEvent;
        public event Action<int>? DbusEvent;
        public event Action<int>? AluEvent;
        public event Action<int>? RbusEvent;
        public event Action<int>? MemoryEvent;
        public event Action<int>? OtherEvent;
        public byte     MAR = 0;
		public string[] MIR = new string[10];
        /// <summary>
        /// Micro Program Memory
        /// ROM memory that holds microinstructions of 36 bits wide.
        /// Key: micro-address
        /// Value: micro-commands
        /// </summary>
		public Dictionary<int, string[]> MPM = new Dictionary<int, string[]>();
        public short IR = 0;

		private int		_mirIndex = 0;
		private Dictionary<string, int> _microcommandsIndexes = new Dictionary<string, int>
		{
			{"None",		0 },

			//SBUS
			{"PdFlagsS",	1 },
			{"PdRgS",		2 },
			{"PdSpS",		3 },
			{"PdTS",		4 },
			{"PdPcS",		5 },
			{"PdIvrS",		6 },
			{"PdAdrS",		7 },
			{"PdMdrS",		8 },
			{"PdR0S",		9 },
			{"PdR1S",		10 },
			{"PdR2S",		11 },
			{"PdR3S",		12 },
			{"PdR4S",		13 },
			{"PdR5S",		14 },
			{"PdR6S",		15 },
			{"PdR7S",		16 },
			{"PdR8S",		17 },
			{"PdR9S",		18 },
			{"PdR10S",		19 },
			{"PdR11S",		20 },
			{"PdR12S",		21 },
			{"PdR13S",		22 },
			{"PdR14S",		23 },
			{"PdR15S",		24 },
            {"PdTNegS",     25 },
			{"Pd0S",		26 },
			{"Pd-1S",		27 },

			//DBUS
			{"PdFlagsD",    1 },
            {"PdRgD",       2 },
            {"PdSpD",       3 },
            {"PdTD",        4 },
            {"PdPcD",       5 },
            {"PdIvrD",      6 },
            {"PdAdrD",      7 },
            {"PdMdrD",      8 },
            {"PdR0D",       9 },
            {"PdR1D",       10 },
            {"PdR2D",       11 },
            {"PdR3D",       12 },
            {"PdR4D",       13 },
            {"PdR5D",       14 },
            {"PdR6D",       15 },
            {"PdR7D",       16 },
            {"PdR8D",       17 },
            {"PdR9D",       18 },
            {"PdR10D",      19 },
            {"PdR11D",      20 },
            {"PdR12D",      21 },
            {"PdR13D",      22 },
            {"PdR14D",      23 },
            {"PdR15D",      24 },
            {"PdMDRNegD",   25 },
            {"Pd0D",        26 },
            {"Pd-1D",       27 },

			//ALU
			{"SBUS",		1 },
            {"DBUS",        2 },
            {"ADD",			3 },
            {"SUB",			4 },
            {"AND",			5 },
            {"OR",			6 },
            {"XOR",			7 },
            {"ASL",			8 },
            {"ASR",			9 },
            {"LSR",			10 },
            {"ROL",			11 },
            {"ROR",			12 },
            {"RLC",			13 },
            {"RRC",			14 },

			//RBUS
			{"PmFlags",     1 },
            {"PmRg",		2 },
            {"PmSp",		3 },
            {"PmT",			4 },
            {"PmPC",		5 },
            {"PmIVR",       6 },
            {"PmADR",       7 },
            {"PmMDR",       8 },
			{"PmFlag0",     9 },
            {"PmFlag1",     10 },
            {"PmFlag2",     11 },
            {"PmFlag3",     12 },

			// MemOp
			{"ICFH",		1 },
            {"RD",			2 },
            {"WR",			3 },

			// OtherOp
            {"+2SP",		1 },
            {"-2SP",		2 },
            {"+2PC",		3 },
            {"A(1)BE0",		4 },
            {"A(1)BE1",		5 },
            {"PdCondA",		6 },
            {"CinPdCondA",	7 },
            {"PdCondL",     8 },
            {"A(1)BVI",     9 },
            {"A(0)BVI",     10 },
            {"A(0)BPO",     11 },
            {"INTA,-2SP",   12 },
            {"A(0)BE,A(0)BI", 13 },

        };

        /// <summary>
        /// Implements logic for stepping through
        /// the Sequencer, thus going through
        /// instruction phases and sending
        /// necessary command signals.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="ACLOWSignal"></param>
        /// <param name="flagsRegister"></param>
		internal void StepMicrocode(int initialState,bool ACLOWSignal, short flagsRegister)
        {
            int state = initialState;

            switch (state)
            {
                case 0:
                    this.MIR = this.MPM[this.MAR];
                    state = 1;
                    break;
                case 1:
                    bool g_function = this.ComputeConditionG(ACLOWSignal, flagsRegister);
                    if (g_function)
                        this.MAR = (byte)(Int32.Parse(this.MIR[9]) + this.ComputeMARIndex());
                    else this.MAR++;

                    int mirALUBits = Int32.Parse(this.MIR[2]);
                    bool aluBIT24 = Convert.ToBoolean(mirALUBits & 1);
                    bool aluBIT25 = Convert.ToBoolean(mirALUBits & (1 << 1));
                    if (!aluBIT24 & !aluBIT25)
                        state = 2;
                    else state = 0;

                    DecodeAndSendCommand();

                    break;
                case 2:
                    state = 3;
                    // Note: this state would normally be reserved for DMA logic
                    // but in the current implementation it shall be ignored.
                    break;

            }
            DecodeAndSendCommand();
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
            bool CILFlag = false; //TBD
            bool carryFlag = Convert.ToBoolean(flagsRegister & (1<<3));
            bool zeroFlag = Convert.ToBoolean(flagsRegister & (1 << 2));
            bool signFLag = Convert.ToBoolean(flagsRegister & (1 << 1));
            bool overflowFlag = Convert.ToBoolean(flagsRegister & (1 << 0));
            int selectValue = Int32.Parse(this.MIR[6]);
            // Note: since we the hardware behaviour of MIR register has been
            // abstracted, the hardware MIR[13:11] bits corespond to
            // the software MIR[6], i.e. Selection Index
            bool hwMIRbit = Convert.ToBoolean(Int32.Parse(this.MIR[8])); // Hardware MIR[7]

            switch (selectValue)
            {
                case 0:
                    g_flag = false; //g <- hardware MIR[7] xor MIR[7]
                    break;
                case 1:
                    g_flag = true; // g<- hardware Not(MIR[7]) xor MIR[7]
                    break;
                case 2:
                    g_flag = (bool)(ACLOWSignal ^ hwMIRbit);
                    break;
                case 3:
                    g_flag = CILFlag ^ hwMIRbit;
                    break;
                case 4:
                    g_flag = carryFlag ^ hwMIRbit;
                    break;
                case 5:
                    g_flag = zeroFlag ^ hwMIRbit;
                    break;
                case 6:
                    g_flag = signFLag ^ hwMIRbit;
                    break;
                case 7:
                    g_flag = overflowFlag ^ hwMIRbit;
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
            int selectValue = Int32.Parse(this.MIR[7]);
            // Note: since we the hardware behaviour of MIR register has been
            // abstracted, the hardware MIR[10:8] bits corespond to
            // software MIR[7] value.

            int instructionClass0 =(int)((this.IR & (1 << 15)) & (this.IR & (1 << 14)));
            //coresponds to hardware CL0
            int instructionClass1 = (int)((this.IR & (1 << 15)) & (this.IR & (1 << 13)));
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
                    marIndex = (byte)((this.IR & (1 << 11)) | (this.IR & (1 << 10)));
                    break;
                case 3:
                    marIndex = (byte)((this.IR & (1 << 5)) | (this.IR & (1 << 4)));
                    break;
                case 4:
                    marIndex = (byte)(
                        (this.IR & (1 << 14))
                        | (this.IR & (1 << 13))
                        | (this.IR & (1 << 12))
                        );
                    break;
                case 5:
                    marIndex = (byte)(
                        (this.IR & (1 << 11))
                        | (this.IR & (1 << 10))
                        | (this.IR & (1 << 9))
                        | (this.IR & (1 << 8))
                        );
                    break;
                case 6:
                    marIndex = (byte)( (
                        (this.IR & (1 << 11))
                        | (this.IR & (1 << 10))
                        | (this.IR & (1 << 9))
                        | (this.IR & (1 << 8))
                        ) <<1);
                    break;
                case 7:
                    int interruptSignal = 0; //TBD
                    marIndex = (byte)(interruptSignal << 2);
                    break;

            }
            return marIndex;
		}
		private void DecodeAndSendCommand()
		{
			switch (_mirIndex) {
				case 0:
                    SbusEvent?.Invoke(_microcommandsIndexes[MIR[_mirIndex]]);
                    break;
				case 1:
                    DbusEvent?.Invoke(_microcommandsIndexes[MIR[_mirIndex]]);
                    break;
				case 2:
                    AluEvent?.Invoke(_microcommandsIndexes[MIR[_mirIndex]]);
                    break;
                case 3:
                    RbusEvent?.Invoke(_microcommandsIndexes[MIR[_mirIndex]]);
                    break;
                case 3:
                    MemoryEvent?.Invoke(_microcommandsIndexes[MIR[_mirIndex]]);
                    break;
				case 4:
                    OtherEvent?.Invoke(_microcommandsIndexes[MIR[_mirIndex]]);
                    break;
			}
			_mirIndex %= 5;
			_mirIndex++;
        }

	}

}