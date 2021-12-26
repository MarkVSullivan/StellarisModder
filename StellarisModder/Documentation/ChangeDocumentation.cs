using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public static class ChangeDocumentation
    {
        private static List<string> changes;

        static ChangeDocumentation()
        {
            changes = new List<string>();
        }

        public static void Add(string NewChange)
        {
            changes.Add(NewChange);
        }

        public static void Clear()
        {
            changes.Clear();
        }

        public static string AllChanges()
        {
            StringBuilder builder = new StringBuilder();
            foreach( string thisLine in changes )
            {
                builder.AppendLine(thisLine);
            }
            return builder.ToString();
        }
    }
}
