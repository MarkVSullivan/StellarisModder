using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    class Program
    {



        static void Main(string[] args)
        {
            // If the stellaris executable isn't found, show an error
            if ( !File.Exists(Stellaris_Constants.Stellaris_Exe))
            {
                Console.WriteLine("Unable to locate your Stellaris folder");
                Console.WriteLine("Looked here: " + Stellaris_Constants.Stellaris_Folder);
                Console.ReadKey();
                return;
            }

            // Continue to show the main menu until the user is done
            int choice = show_main_menu();
            while (choice != 3)
            {
                switch( choice )
                {
                    case 1:
                        AchievementEnablerHelper.Enable_Achievements();
                        break;

                    case 2:
                        ResourceFilesController.Create_Updated_Files();
                        break;

                    case 3:
                        return;
                }

                choice = show_main_menu();
            }
        }

        private static void add_blank_lines(int lines)
        {
            for (int i = 0; i < lines; i++)
                Console.WriteLine();
        }

        private static int show_main_menu()
        {
            add_blank_lines(3);

            Console.WriteLine("Mark's Stellaris Modification Helper");
            Console.WriteLine();
            Console.WriteLine("    1. Enable achievements");
            Console.WriteLine("    2. Create mod");
            Console.WriteLine("    3. Exit");
            Console.WriteLine();
            Console.Write("Enter your choice: ");
            var keyInfo = Console.ReadKey();

            add_blank_lines(2);

            if (Int32.TryParse(keyInfo.KeyChar.ToString(), out int choice))
                return choice;
            else
                return -1;
        }


    }
}
