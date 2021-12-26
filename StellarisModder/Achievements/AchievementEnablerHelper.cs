using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public static class AchievementEnablerHelper
    {
        private static List<string> hex_matches;

        public static void Enable_Achievements()
        {
            // Get the matches from the backup file
            if (hex_matches == null)
            {
                hex_matches = find_possible_hex_locations();
            }

            // Show them
            Console.WriteLine();
            Console.WriteLine($"Found {hex_matches.Count} matches in the file.");
            Console.WriteLine();
            Console.WriteLine($"    0. No replacement - restore original executable from backup");
            for (int i = 1; i <= hex_matches.Count; i++)
            {
                Console.WriteLine($"    {i}. Match found at {hex_matches[i - 1]}");
            }
            Console.WriteLine();
            Console.Write("Choose location to replace: ");
            var keyInfo = Console.ReadKey();
            add_blank_lines(2);

            if (Int32.TryParse(keyInfo.KeyChar.ToString(), out int choice))
            {
                if (choice >= 0 && choice <= hex_matches.Count)
                {
                    int index = -1;
                    int offset = 0;
                    if (choice >= 1)
                    {
                        string match = hex_matches[choice - 1];
                        string[] splitter = match.Split(':');
                        index = Int32.Parse(splitter[0].Trim());
                        string replacement = splitter[1].Trim();
                        offset = (replacement.Replace(" ", "").Length - 4) / 2;

                        Console.WriteLine($"Will make replacement at {index} (offset {offset})");
                    }


                    //Console.WriteLine($"Replace " + replacement);
                    //Console.WriteLine($"Will replace 85 C0 with 33 C0");

                    copy_stellaris_executable(index, offset);
                }
            }
        }

        private static void copy_stellaris_executable(int index, int offset)
        {
            // Delete existing executable file
            if (!File.Exists(Stellaris_Constants.Stellaris_Exe))
                File.Delete(Stellaris_Constants.Stellaris_Exe);

            Console.WriteLine();
            Console.Write("Copying from backup file.");

            // Open stream to backup file
            FileStream in_stream = new FileStream(Stellaris_Constants.Stellaris_Backup, FileMode.Open);

            // Create new executable file
            FileStream out_stream = new FileStream(Stellaris_Constants.Stellaris_Exe, FileMode.Create);

            // Copy each hex value for now
            int hexIn;
            int index_since_last_period = 0;
            for (int i = 0; (hexIn = in_stream.ReadByte()) != -1; i++)
            {
                if ((i == index + offset) && (string.Format("{0:X2}", hexIn) == "85"))
                {
                    out_stream.WriteByte((byte)51);
                    Console.Write("*");
                }
                else
                {
                    out_stream.WriteByte((byte)hexIn);
                }

                // Add a period?
                index_since_last_period++;
                if (index_since_last_period > 1000000)
                {
                    Console.Write(".");
                    index_since_last_period = 0;
                }
            }

            // Close streams
            out_stream.Close();
            in_stream.Close();

            add_blank_lines(2);
            Console.WriteLine("Copy Complete");
        }


        private static void ensure_backup_exists()
        {
            // Make backup if one doesn't exist
            if (!File.Exists(Stellaris_Constants.Stellaris_Backup))
            {
                Console.WriteLine("No backup found, copying the executable file to stellaris_exe.bak");
                File.Copy(Stellaris_Constants.Stellaris_Exe, Stellaris_Constants.Stellaris_Backup);
            }
        }

        private static List<string> find_possible_hex_locations()
        {
            ensure_backup_exists();

            Console.Write("Looking for hex locations in executable.");

            // Open stream to executable file
            FileStream fs = new FileStream(Stellaris_Constants.Stellaris_Backup, FileMode.Open);
            int hexIn;

            string hex1 = "";
            string hex2 = "";
            string hex3 = "";
            string hex4 = "";
            string hex5 = "";
            int offset = -1;
            int index_since_last_period = 0;
            int matches_found = 0;
            var resultBuilder = new StringBuilder();

            List<string> matches = new List<string>();

            for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
            {
                // Save the last ones
                hex5 = hex4;
                hex4 = hex3;
                hex3 = hex2;
                hex2 = hex1;

                // Get the new value
                hex1 = string.Format("{0:X2}", hexIn);

                // Was this a partial match?
                if (hex5 == "48" && hex4 == "8B" && hex3 == "12" && hex2 == "48" && hex1 == "8D")
                {
                    resultBuilder.Append(hex5 + " " + hex4 + " " + hex3 + " " + hex2 + " " + hex1);

                    offset = i - 4;


                    for (int j = 0; j < 11; j++)
                    {
                        resultBuilder.Append(" " + string.Format("{0:X2}", fs.ReadByte()));
                    }

                    string hex12 = string.Format("{0:X2}", fs.ReadByte());
                    string hex13 = string.Format("{0:X2}", fs.ReadByte());
                    string hex14 = string.Format("{0:X2}", fs.ReadByte());

                    i += 14;

                    if (hex12 == "85" && hex13 == "C0")
                    {
                        matches_found++;
                        resultBuilder.Append(" " + hex12 + " " + hex13);

                        matches.Add($"{offset} : {resultBuilder.ToString()}");
                        Console.Write("*");
                    }
                    else if (hex13 == "85" && hex14 == "C0")
                    {
                        matches_found++;
                        resultBuilder.Append(" " + hex12 + " " + hex13 + " " + hex14);

                        matches.Add($"{offset} : {resultBuilder.ToString()}");
                        Console.Write("*");
                    }

                    resultBuilder.Clear();
                }

                // Add a period?
                index_since_last_period++;
                if (index_since_last_period > 1000000)
                {
                    Console.Write(".");
                    index_since_last_period = 0;
                }
            }

            fs.Close();
            Console.WriteLine();
            return matches;
        }

        private static void add_blank_lines(int lines)
        {
            for (int i = 0; i < lines; i++)
                Console.WriteLine();
        }
    }
}
