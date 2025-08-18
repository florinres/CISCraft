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
        public InterruptController()
        {
            prioritisedIRQs = new Dictionary<string, bool>();
            prioritisedIRQs.Add("I0p", false);
            prioritisedIRQs.Add("I1p", false);
            prioritisedIRQs.Add("I2p", false);
            prioritisedIRQs.Add("I3p", false);
        }
        /// <summary>
        /// Checks interupt requests coming from the I/O modules
        /// and the state of CPU exceptions and returns the
        /// appropriate global interrupt request signal
        /// </summary>
        /// <param name="irqs"></param>
        /// <param name="exceptions"></param>
        /// <returns>globalIRQ, prioritisedIRQs[]</returns>
        public Dictionary<string,bool> CheckInterruptSignals(bool []irqs, bool[] exceptions)
        {
            bool exceptionExclude = !(exceptions[0] | exceptions[1] | exceptions[2] | exceptions[3]);
            //if any exception is detected, exclude all of the interrupt requests

            prioritisedIRQs["I0p"] = irqs[0] & exceptionExclude;
            prioritisedIRQs["I1p"] = irqs[1] & !irqs[0] & exceptionExclude;
            prioritisedIRQs["I2p"] = irqs[2] & !irqs[1] & !irqs[0] & exceptionExclude;
            prioritisedIRQs["I3p"] = irqs[3] & irqs[2] & !irqs[1] & !irqs[0] & exceptionExclude;
            return prioritisedIRQs;
        }
        /// <summary>
        /// Prioritises the detected interruptions.
        /// </summary>
        /// <param name="detectedExceptions"></param>
        /// <returns></returns>
        private bool[] PrioritiseExceptions(bool [] detectedExceptions)
        {
            bool [] prioritisedExceptions = new bool[4];

            prioritisedExceptions[0] = detectedExceptions[0];
            prioritisedExceptions[1] = detectedExceptions[1] & !prioritisedExceptions[0];
            prioritisedExceptions[2] = prioritisedExceptions[2] & !prioritisedExceptions[1] & !prioritisedExceptions[0];
            prioritisedExceptions[3] = prioritisedExceptions[3] & !prioritisedExceptions[2] & !prioritisedExceptions[1] & !prioritisedExceptions[0];
            return prioritisedExceptions;
        }
        /// <summary>
        /// Computes value for interrupt vector used for select the correct
        /// interrupt handle.
        /// </summary>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        public short ComputeInterruptVector(bool[] exceptions)
        {
            short interruptVector = 0;

            bool[] prioritisedExceptions = PrioritiseExceptions(exceptions);
            int foo = Convert.ToInt16(exceptions[1]);
            interruptVector |= (short)(Convert.ToInt16(exceptions[1]) | Convert.ToInt16(exceptions[3]) | Convert.ToInt16(prioritisedIRQs["I1p"]) | Convert.ToInt16(prioritisedIRQs["I3p"]) << 1);
            interruptVector |= (short)(Convert.ToInt16(exceptions[2]) | Convert.ToInt16(exceptions[3]) | Convert.ToInt16(prioritisedIRQs["I2p"]) | Convert.ToInt16(prioritisedIRQs["I3p"]) << 2);
            interruptVector |= (short)(Convert.ToInt16(prioritisedIRQs["I0p"]) | Convert.ToInt16(prioritisedIRQs["I1p"]) | Convert.ToInt16(prioritisedIRQs["I2p"]) | Convert.ToInt16(prioritisedIRQs["I3p"]) << 3);
            return interruptVector;
        }
    }
}
