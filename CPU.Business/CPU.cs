using System;

namespace CPU.Business
{ 
	public class CPU
	{
		public ushort[] Registers = new ushort[21];
		public ushort SBUS, DBUS, RBUS;
		private ControlUnit _controlUnit;
		public CPU()
		{
			_controlUnit = new ControlUnit();
            _controlUnit.SbusEvent   += OnSbusEvent;
            _controlUnit.DbusEvent   += OnDbusEvent;
            _controlUnit.AluEvent    += OnAluEvent;
            _controlUnit.MemoryEvent += OnMemoryEvent;
            _controlUnit.OtherEvent  += OnOtherEvent;
        }
		public void StepMicrocode()
		{
            _controlUnit.StepMicrocode();
        }
        private void OnSbusEvent(int index)
        {
            throw new NotImplementedException();
        }
        private void OnDbusEvent(int index)
        {
            throw new NotImplementedException();
        }
        private void OnAluEvent(int index)
        {
            throw new NotImplementedException();
        }
        private void OnMemoryEvent(int index)
        {
            throw new NotImplementedException();
        }
        private void OnOtherEvent(int index)
        {
            throw new NotImplementedException();
        }
    }
}

