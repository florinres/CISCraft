using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler.Business
{
    [Language("casm", "1.0", "Assembly language grammar")]
    internal class CiscGrammar : Grammar
    {
        internal CiscGrammar() : base(false)
        {
            var number         = new NumberLiteral("number", NumberOptions.AllowSign);
            var str            = new IdentifierTerminal("identifier");
            KeyTerm COMMA      = ToTerm(",");
            KeyTerm COLON      = ToTerm(":");
            KeyTerm LBRACKET   = ToTerm("[");
            KeyTerm RBRACKET   = ToTerm("]");

            NonTerminal b1Oppcodes     = new NonTerminal("B1Oppcodes");
            NonTerminal b2Oppcodes     = new NonTerminal("B2Oppcodes");
            NonTerminal b3Oppcodes     = new NonTerminal("B3Oppcodes");
            NonTerminal b4Oppcodes     = new NonTerminal("B4Oppcodes");
            NonTerminal b1Instr        = new NonTerminal("B1Instr");
            NonTerminal b2Instr        = new NonTerminal("B2Instr");
            NonTerminal b3Instr        = new NonTerminal("B3Instr");
            NonTerminal b4Instr        = new NonTerminal("B4Instr");
            NonTerminal instruction    = new NonTerminal("Instr");
            NonTerminal register       = new NonTerminal("Register");
            NonTerminal mem            = new NonTerminal("MemoryAccess");
            NonTerminal prog           = new NonTerminal("Program");
            NonTerminal b1Operand1     = new NonTerminal("B1Operand1");
            NonTerminal b1Operand2     = new NonTerminal("B1Operand2");
            NonTerminal b2Operand      = new NonTerminal("B2Operand");
            NonTerminal b3Operand      = new NonTerminal("B3Operand");
            NonTerminal label          = new NonTerminal("Label");
            NonTerminal instrList      = new NonTerminal("InstructionList");
            NonTerminal proc           = new NonTerminal("Proc");
            NonTerminal endp           = new NonTerminal("Endp");
            var comment                = new CommentTerminal("comment", ";", "\n","\n\r");

            b1Oppcodes.Rule =
                ToTerm("MOV") |
                ToTerm("ADD") |
                ToTerm("SUB") |
                ToTerm("CMP") |
                ToTerm("AND") |
                ToTerm("OR")  |
                ToTerm("XOR");

            b2Oppcodes.Rule =
                ToTerm("CLR")  |
                ToTerm("NEG")  |
                ToTerm("INC")  |
                ToTerm("DEC")  |
                ToTerm("ASL")  |
                ToTerm("ASR")  |
                ToTerm("LSR")  |
                ToTerm("ROL")  |
                ToTerm("ROR")  |
                ToTerm("RLC")  |
                ToTerm("RRC")  |
                ToTerm("PUSH") |
                ToTerm("POP");

            b3Oppcodes.Rule =
                ToTerm("BNE") |
                ToTerm("BEQ") |
                ToTerm("BPL") |
                ToTerm("BMI") |
                ToTerm("BCS") |
                ToTerm("BCC") |
                ToTerm("BVS") |
                ToTerm("BVC") |
                ToTerm("JMP") |
                ToTerm("CALL");

            b4Oppcodes.Rule =
                ToTerm("CLC")    |
                ToTerm("SEC")    |
                ToTerm("NOP")    |
                ToTerm("EI")     |
                ToTerm("DI")     |
                ToTerm("RET")    |
                ToTerm("RETI")   |
                ToTerm("HALT")   |
                ToTerm("WAIT")   |
                ToTerm("PUSHPC") |
                ToTerm("POPPC")  |
                ToTerm("PUSHF")  |
                ToTerm("POPF");

            register.Rule =
                ToTerm("R0")  |
                ToTerm("R1")  |
                ToTerm("R2")  |
                ToTerm("R3")  |
                ToTerm("R4")  |
                ToTerm("R5")  |
                ToTerm("R6")  |
                ToTerm("R8")  |
                ToTerm("R9")  |
                ToTerm("R10") |
                ToTerm("R12") |
                ToTerm("R13") |
                ToTerm("R14") |
                ToTerm("R15");

            NonGrammarTerminals.Add(comment);

            mem.Rule  = (/*optional*/ number.Q() + LBRACKET + register + RBRACKET);
            proc.Rule = ToTerm("PROC") + str;
            endp.Rule = ToTerm("ENDP") + str;
            label.Rule = str + COLON;

            b1Operand1.Rule  = register | mem;
            b1Operand2.Rule  = register | number | mem;
            b2Operand.Rule   = register | number | mem | str;
            b3Operand.Rule   = number   | str;

            b1Instr.Rule = b1Oppcodes + b1Operand1 + COMMA + b1Operand2;
            b2Instr.Rule = b2Oppcodes + b2Operand;
            b3Instr.Rule = b3Oppcodes + b3Operand;
            b4Instr.Rule = b4Oppcodes;
            instruction.Rule = b1Instr | b2Instr | b3Instr | b4Instr | label | proc | endp;

            instrList.Rule = MakeStarRule(instrList, null, instruction);
            prog.Rule      = instrList;
            Root           = prog;

            MarkPunctuation("\n", "\r\n", ",", ":", "[", "]");
            RegisterBracePair("[", "]");
        }
    }
}
