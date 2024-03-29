﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public static class ResourceFilesController
    {
        public static void Create_Updated_Files()
        {
            // Clear any change documentation (if this was just run)
            ChangeDocumentation.Clear();

            // Get the folders for the process from the constants
            string mod_folder = Stellaris_Constants.Instructions_Folder;
            string output_folder = Stellaris_Constants.Mod_Output_Folder;

            // Step through each separate mod, creating them
            string[] mod_dirs = Directory.GetDirectories(mod_folder);
            foreach( string thisModDir in mod_dirs )
            {
                // Clear any existing change documentation
                ChangeDocumentation.Clear();

                // Find the directory name
                string dirName = Path.GetFileName(thisModDir);
                Console.WriteLine(dirName.ToUpper());

                // Find the write folders
                string modOutputFolder = Path.Combine(output_folder, dirName);
                if (!Directory.Exists(modOutputFolder))
                    Directory.CreateDirectory(modOutputFolder);


                recurse_through_instructions(thisModDir, modOutputFolder, Stellaris_Constants.Stellaris_Folder, String.Empty);

                // Show the change log
                Console.WriteLine();
                Console.WriteLine(ChangeDocumentation.AllChanges());
                Console.WriteLine();
                Console.WriteLine();
            }

        }

        private static void recurse_through_instructions(string instruction_folder, string output_folder, string source_folder, string relative)
        {
            // Update console with work
            if (!String.IsNullOrEmpty(relative))
                Console.WriteLine(relative);

            // Ensure output folder exists
            if (!Directory.Exists(output_folder))
                Directory.CreateDirectory(output_folder);

            // Handle instructions
            string[] files = Directory.GetFiles(instruction_folder);
            foreach( string thisInstructionFile in files )
            {
                string fileName = Path.GetFileName(thisInstructionFile);
                string sourceFile = Path.Combine(source_folder, fileName);
                string outputFile = Path.Combine(output_folder, fileName);
                string relativeFile = relative + "\\" + fileName;

                // Source file no longer exists?
                if ( !File.Exists(sourceFile))
                {
                    Console.WriteLine("ERROR: Unable to find source file ( " + relativeFile + ")");
                    continue;
                }

                // Make destination folder
                if (!Directory.Exists(output_folder))
                    Directory.CreateDirectory(output_folder);

                // Add log to console
                Console.WriteLine(relativeFile);
               // File.Copy(sourceFile, outputFile, true);

                // Now, read the instructions and output file and make replacements as needed
                ResourceFile file = new ResourceFile(sourceFile);

                // Update the contents based on the instructions
                file.Update(thisInstructionFile);

                // Write the new file out
                file.Write(outputFile);
            }

            // Handle subdirectories
            string[] directories = Directory.GetDirectories(instruction_folder);
            foreach (string thisDirectory in directories)
            {
                string dirName = Path.GetFileName(thisDirectory);
                string outputDir = Path.Combine(output_folder, dirName);
                string sourceDir = Path.Combine(source_folder, dirName);
                string instrDir = Path.Combine(instruction_folder, dirName);
                string relativeSub = relative + "\\" + dirName;
                recurse_through_instructions(instrDir, outputDir, sourceDir, relativeSub);
            }
        }
    }
}
