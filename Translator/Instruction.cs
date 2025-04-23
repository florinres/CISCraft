namespace Translator
{
    public class Instruction
    {
        public static object DetermineClass(string[] IR)
        {
            string opcode = IR[0];
            if (opcode.StartsWith("1010"))
            {
                return InstructionClass.ClassB;
            }
            if (opcode.StartsWith("1100"))
            {
                return InstructionClass.ClassC;
            }
            if (opcode.StartsWith("1110"))
            {
                return InstructionClass.ClassD;
            }
            else
            {
                return InstructionClass.ClassA;
            }
        }

        public enum InstructionClass
        {
            ClassA,
            ClassB,
            ClassC,
            ClassD
        }
    }
}
