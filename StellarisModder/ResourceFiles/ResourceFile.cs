using StellarisModder.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public class ResourceFile
    {
        private readonly List<ResourceFileEntity> entities;

        private bool replaceEntireFile = true;

        public ResourceFile(string SourceFile)
        {
            // Get the file contents
            string sourceContent = File.ReadAllText(SourceFile);
            entities = ResourceFileEntityParser.Parse(sourceContent);
        }

        public void Update(string instructionFile)
        {
            // Get the instruction file details
            InstructionSet instructions = InstructionSet.Read(instructionFile);

            // Peek into the first line to see if this is going to attempt a partial replacement
            if ( instructions.Peek() != null && instructions.Peek().Equals("@partial", StringComparison.OrdinalIgnoreCase))
            {
                replaceEntireFile = false;
                instructions.Pop();
            }

            // Loop through the instructions
            while ( instructions.Peek() != null )
            {
                // Get action to handle the remaining instructinos
                IAction action = ActionFactory.GetAction(instructions);

                // The action could be null if this was comments, or an empty line
                // This doesn't NECESSARILY mean the end of instructions was encountered
                if (action == null) continue;

                // Let the action manage itself
                action.Apply(instructions, this);
            }
        } 

        public void Write(string OutputFile)
        {
            if (File.Exists(OutputFile))
                File.Delete(OutputFile);

            // Open stream to the file
            var writer = new StreamWriter(OutputFile, false);

            // Write each entity
            foreach( var entity in entities)
            {
                if (entity.Changed || replaceEntireFile)
                {
                    // Write the value of the entity
                    writer.WriteLine(entity.Value);
                }
            }

            // Flush and close
            writer.Flush();
            writer.Close();
        }

        public ResourceFileEntity GetEntity(string EntityName)
        {
            return GetEntity(EntityName, null);
        }

        public ResourceFileEntity GetEntity( string EntityName, string id )
        {
            foreach(ResourceFileEntity entity in entities )
            {
                if (( entity.Type != ResourceFileEntityType.Comments ) && ( entity.EntityName.Equals(EntityName, StringComparison.OrdinalIgnoreCase )))
                {
                    if (!String.IsNullOrEmpty(id))
                    {
                        // Need to match the next line 
                        string[] split = entity.Value.Split('\r');
                        if (( split.Length > 1 ) && ( split[1].IndexOf("id = " + id.Trim()) >= 0))
                        {
                            return entity;
                        }
                    }
                    else
                    {
                        return entity;
                    }
                }
            }

            return null;
        }

        public List<ResourceFileEntity> Entities
        {
            get { return entities; }
        }
    }
}
