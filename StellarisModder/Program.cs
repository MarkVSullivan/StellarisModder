using System;
using System.IO;

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
            Console.WriteLine("Mark's Stellaris Modification Helper");
            add_blank_lines(1);
            Console.WriteLine("Creating updated mod files now");
            add_blank_lines(3);

            ResourceFilesController.Create_Updated_Files();

            add_blank_lines(3);
            Console.WriteLine("Complete!");
            add_blank_lines(1);
            Console.WriteLine("Enter any key to continue");
            Console.ReadKey();
        }

        private static void add_blank_lines(int lines)
        {
            for (int i = 0; i < lines; i++)
                Console.WriteLine();
        }
    }
}
