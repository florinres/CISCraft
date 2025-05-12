using System;

namespace CPU.Business
{
	public class ControlUnit
	{
        public event Action<int>? SbusEvent;
        public event Action<int>? DbusEvent;
        public event Action<int>? AluEvent;
        public event Action<int>? MemoryEvent;
        public event Action<int>? OtherEvent;
        public byte     MAR = 0;
		public string[] MIR = new string[10];
		public string[] MPM = new string[1180];

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
			{"Pd0S",		25 },
			{"Pd-1S",		26 },
            {"PdTNegS",     27 },

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
            {"Pd0D",        25 },
            {"Pd-1D",       26 },
            {"PdMDRNegD",   27 },

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
		// Functie secventiator
		internal void StepMicrocode()
        {
			DecodeAndSendCommand();
        }

        private bool ComputeConditionF()
		{
			return false;
		}

		private bool ComputeConditionG()
		{
			return true;
		}
		// For MAR register
		private byte ComputeIndex()
		{
			return 0;
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