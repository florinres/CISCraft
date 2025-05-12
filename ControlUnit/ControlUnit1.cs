using ControlUnit.Components;
using ControlUnit.Registers;

namespace ControlUnit
{
    public class ControlUnit1
    {
        private Generator generator;
        private Sequencer sequencer;
        private RamificationConditionRegister ramificationConditionRegister;
        private IndexSelectionBlock indexSelectionBlock;
        private MARAddressSum marAddressSum;
        private MAR mar;
        private MIR mir;
        private MicrocommandDecodifier microcommandDecodifier;
        public ControlUnit1()
        {
            generator = new Generator();
            sequencer = new Sequencer();
            ramificationConditionRegister = new RamificationConditionRegister();
            indexSelectionBlock = new IndexSelectionBlock();
            marAddressSum = new MARAddressSum();
            mar = new MAR();
            mir = new MIR();
            microcommandDecodifier = new MicrocommandDecodifier();
        }
    }
}
