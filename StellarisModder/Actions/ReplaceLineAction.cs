using StellarisModder.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder.Actions
{
    public class ReplaceLineAction : IAction
    {
        public bool Apply(InstructionSet Instructions, ResourceFile File)
        {
            // Read the instruction line
            string instruction = Instructions.Pop().Trim();

            // Was this scoped within a single entity?
            string entity = String.Empty;
            if ( instruction.IndexOf(" ") > 0 )
            {
                entity = instruction.Substring(instruction.IndexOf(" ")).Trim();                
            }

            // Get next line (line to replace)
            string lineToReplace = Instructions.Pop().Trim();

            // Next line should be @WITH.. but verify
            string peek = Instructions.Peek();
            string replaceWith = String.Empty;
            if (( peek != null ) && ( peek.IndexOf("@WITH", StringComparison.OrdinalIgnoreCase) == 0 ))
            {
                Instructions.Pop();
                replaceWith = Instructions.Pop().Trim();
            }

            // Gathered all the data, so run the replacements
            if (!String.IsNullOrEmpty(entity))
            {
                if ( !replace_within_entity(File, entity, lineToReplace, replaceWith))
                {
                    ModderLogger.Log("Unable to replace line in entity " + entity);
                    return false;
                }
            }
            else
            {
                if ( !replace_throughout(File, lineToReplace, replaceWith))
                {
                    ModderLogger.Log("Never found line to replace in file");
                    return false;
                }
            }

            return true;
        }

        private bool replace_within_entity(ResourceFile file, string entityName, string lineToReplace, string replaceWith)
        {
            // This may have sub-entities to deal with.. let's check
            if (entityName.IndexOf(">") < 0)
            {
                string id = null;
                if (entityName.IndexOf("#") > 0)
                {
                    id = entityName.Substring(entityName.IndexOf("#") + 1);
                    entityName = entityName.Substring(0, entityName.IndexOf("#"));
                }

                ResourceFileEntity entity = file.GetEntity(entityName, id);
                if (entity == null) return false;

                return do_entity_replacement(entity, lineToReplace, replaceWith);
            }
            else
            {
                int seperatorIndex = entityName.IndexOf('>');
                string parentEntityName = entityName.Substring(0, seperatorIndex);
                string remainingEntityChain = entityName.Substring(seperatorIndex + 1);

                var parentEntity = file.GetEntity(parentEntityName);

                if (parentEntity == null) return false;

                bool returnValue = replace_within_entity_recursive(parentEntity, remainingEntityChain, lineToReplace, replaceWith, 1);

                if (parentEntity.SubEntities != null)
                    parentEntity.Collapse(0);
                parentEntity.Changed = true;

                return returnValue;
            }
        }

        public bool do_entity_replacement(ResourceFileEntity entity, string lineToReplace, string replaceWith)
        {
            if (entity.Value.IndexOf("\t" + lineToReplace + "\r") < 0)
                return false;

            string newValue = entity.Value.Replace("\t" + lineToReplace + "\r", "\t" + replaceWith + "\r");
            entity.Value = newValue;
            entity.Changed = true;
            return true;
        }

        private bool replace_within_entity_recursive(ResourceFileEntity entity, string entityChain, string lineToReplace, string replaceWith, int depth)
        {
            if (entity == null)
                return false;

            bool returnValue = false;

            int seperatorIndex = entityChain.IndexOf('>');
            if (seperatorIndex < 0)
            {
                var childEntity = entity.GetSubEntity(entityChain.Trim());
                returnValue = do_entity_replacement(childEntity, lineToReplace, replaceWith);

            }
            else
            {
                string nextEntityName = entityChain.Substring(0, seperatorIndex);
                string remainingEntityChain = entityChain.Substring(seperatorIndex + 1);
                var nextEntity = entity.GetSubEntity(nextEntityName);
                returnValue = replace_within_entity_recursive(nextEntity, remainingEntityChain, lineToReplace, replaceWith, depth + 1);
                nextEntity.Changed = true;
            }

            return returnValue;
        }

        private bool replace_throughout(ResourceFile file, string lineToReplace, string replaceWith)
        {
            bool lineFound = false;

            foreach( ResourceFileEntity entity in file.Entities)
            {
                if (entity.Type == ResourceFileEntityType.Comments)
                    continue;

                if (( entity.Value.IndexOf("\r") < 0 ) && (entity.Value.IndexOf(lineToReplace) >= 0))
                {
                    lineFound = true;

                    string newValue = entity.Value.Replace(lineToReplace, replaceWith);
                    entity.Value = newValue;
                    entity.Changed = true;
                }
                else if (entity.Value.IndexOf("\t" + lineToReplace + "\r") >= 0)
                {
                    lineFound = true;

                    string newValue = entity.Value.Replace("\t" + lineToReplace + "\r", "\t" + replaceWith + "\r");
                    entity.Value = newValue;
                    entity.Changed = true;
                }
            }

            return lineFound;
        }
    }
}
