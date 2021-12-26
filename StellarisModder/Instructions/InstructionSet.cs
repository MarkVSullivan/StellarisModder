using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public class InstructionSet
    {
        private List<string> instructions;
        private int currentIndex;

        public InstructionSet()
        {
            instructions = new List<string>();
            currentIndex = -1;
        }

        public void Push(string NewInstruction)
        {
            instructions.Add(NewInstruction);
        }

        public string Peek()
        {
            if (currentIndex + 1 < instructions.Count)
                return instructions[currentIndex + 1];

            return null;
        }

        public string Pop()
        {
            currentIndex++;

            if (currentIndex < instructions.Count)
                return instructions[currentIndex];

            return null;
        }

        public static InstructionSet Read(string InstructionFile)
        {
            var returnValue = new InstructionSet();

            string[] text = File.ReadAllLines(InstructionFile);
            
            foreach( string thisLine in text )
            {
                returnValue.Push(thisLine);
            }

            return returnValue;
        }
    }
}
