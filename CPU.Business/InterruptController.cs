using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU.Business
{
    class InterruptController
    {
        Dictionary<string, bool> prioritisedIRQs;
        Dictionary<string, bool> prioritisedExceptions;
        public InterruptController()
        {
            prioritisedIRQs = new Dictionary<string, bool>();
            prioritisedIRQs.Add("I0p", false);
            prioritisedIRQs.Add("I1p", false);
            prioritisedIRQs.Add("I2p", false);
            prioritisedIRQs.Add("I3p", false);

            prioritisedExceptions = new Dictionary<string, bool>();
            prioritisedExceptions.Add("ACLOW", false);
            prioritisedExceptions.Add("CIL", false);
            prioritisedExceptions.Add("Reserved0", false);
            prioritisedExceptions.Add("Reserved1", false);
        }
        /// <summary>
        /// Checks interrupt requests coming from the I/O modules,
        /// the state of CPU exceptions and sets the internal values
        /// for the prioritised exceptions.
        /// </summary>
        /// <param name="irqs"></param>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        public void CheckInterruptSignals(bool[] irqs, bool[] exceptions)
        {
            bool exceptionExclude = !(exceptions[0] | exceptions[1] | exceptions[2] | exceptions[3]);
            //if any exception is detected, exclude all of the interrupt requests

            prioritisedIRQs["I0p"] = irqs[0] & exceptionExclude;
            prioritisedIRQs["I1p"] = irqs[1] & !irqs[0] & exceptionExclude;
            prioritisedIRQs["I2p"] = irqs[2] & !irqs[1] & !irqs[0] & exceptionExclude;
            prioritisedIRQs["I3p"] = irqs[3] & !irqs[2] & !irqs[1] & !irqs[0] & exceptionExclude;;
        }
        /// <summary>
        /// Returns a dictionary containing the pair
        /// interrupt_number:priority
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, bool> GetPrioritisedIRQStates()
        {
            return new Dictionary<string, bool>(prioritisedIRQs);
        }
        /// <summary>
        //Returns a dictionary containing the pair
        /// exception:priority
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, bool> GetPrioritisedExceptions()
        {
            return new Dictionary<string, bool>(prioritisedExceptions);
        }
        /// <summary>
        /// Prioritises the detected interruptions.
        /// </summary>
        /// <param name="detectedExceptions"></param>
        /// <returns></returns>
        private void PrioritiseExceptions(bool[] detectedExceptions)
        {
            prioritisedExceptions["ACLOW"] = detectedExceptions[0];
            prioritisedExceptions["CIL"] = detectedExceptions[1] & !prioritisedExceptions["ACLOW"];
            prioritisedExceptions["Reserved0"] = detectedExceptions[2] & !prioritisedExceptions["CIL"] & !prioritisedExceptions["ACLOW"];
            prioritisedExceptions["Reserved1"] = detectedExceptions[3] & !prioritisedExceptions["Reserved0"] & !prioritisedExceptions["CIL"] & !prioritisedExceptions["ACLOW"];
        }
        /// <summary>
        /// Computes value for interrupt vector used for select the correct
        /// interrupt handle.
        /// </summary>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        public ushort ComputeInterruptVector(bool[] exceptions)
        {
            ushort interruptVector = 0;
            PrioritiseExceptions(exceptions);

            ushort bit1 = (ushort)(Convert.ToInt16(prioritisedExceptions["CIL"]) | Convert.ToInt16(prioritisedExceptions["Reserved1"]) | Convert.ToInt16(prioritisedIRQs["I1p"]) | Convert.ToInt16(prioritisedIRQs["I3p"]));
            ushort bit2 = (ushort)(Convert.ToInt16(prioritisedExceptions["Reserved0"]) | Convert.ToInt16(prioritisedExceptions["Reserved1"]) | Convert.ToInt16(prioritisedIRQs["I2p"]) | Convert.ToInt16(prioritisedIRQs["I3p"]));
            ushort bit3 = (ushort)(Convert.ToInt16(prioritisedIRQs["I0p"]) | Convert.ToInt16(prioritisedIRQs["I1p"]) | Convert.ToInt16(prioritisedIRQs["I2p"]) | Convert.ToInt16(prioritisedIRQs["I3p"]));
            interruptVector |= (ushort)(bit1 << 1);
            interruptVector |= (ushort)(bit2 << 2);
            interruptVector |= (ushort)(bit3 << 3);
            return interruptVector;
        }
    }
}
