using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder.Actions
{
    public static class ActionFactory
    {
        public static IAction GetAction(InstructionSet Instructions)
        {
            string peek = Instructions.Peek();

            // If null or empty, return null
            if (String.IsNullOrWhiteSpace(peek))
            {
                Instructions.Pop();
                return null;
            }

            // Comment?
            peek = peek.Trim();
            if (peek[0] == '#')
            {
                if (peek.Length > 1)
                {
                    ChangeDocumentation.Add(peek.Substring(1).Trim());
                }
                Instructions.Pop();
                return null;
            }

            // Look for an action
            if (peek.IndexOf("@LINEREPLACE") == 0)
                return new ReplaceLineAction();

            if (peek.IndexOf("@REPLACE") == 0)
                return new ReplaceEntityAction();

            if (peek.IndexOf("@ADD") == 0)
                return new AddAction();


            // No match.. return null.. but go to next line
            Instructions.Pop();
            return null;
        }
    }
}
