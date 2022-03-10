using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public static class Stellaris_Constants
    {
        public const string Stellaris_Folder = @"C:\Program Files (x86)\Steam\steamapps\common\Stellaris";

        public const string Instructions_Folder = @"C:\Repos\StellarisModder\StellarisModder\Modifications";

        public const string Mod_Output_Folder = @"C:\Repos\StellarisModder\Mod\";

        public readonly static string Stellaris_Exe;
        public readonly static string Stellaris_Backup;

        static Stellaris_Constants()
        {
            // Find the files
            Stellaris_Exe = Path.Combine(Stellaris_Folder, "stellaris.exe");
            Stellaris_Backup = Path.Combine(Stellaris_Folder, "stellaris_exe.bak");
        }

    }
}
