using StellarisModder.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder.Actions
{
    public class ReplaceEntityAction : IAction
    {
        public bool Apply(InstructionSet Instructions, ResourceFile File)
        {
            // Read the instruction line
            string instruction = Instructions.Pop().Trim();

            // Was this scoped within a single entity?
            string entityName = String.Empty;
            string id = String.Empty;
            if (instruction.IndexOf(" ") > 0)
            {
                entityName = instruction.Substring(instruction.IndexOf(" ")).Trim();
            }

            // If not, this is an error (just replcae the whole file then)
            if ( String.IsNullOrEmpty(entityName))
            {
                ModderLogger.Log("Must indicate which entity to replace");
                return false;
            }

            // Get the lines to replace with
            var valueBldr = new StringBuilder();
            string next_line = Instructions.Peek();
            string newEntityName = next_line.Split('=')[0].Trim();
            while (( next_line != null ) && (( next_line.Length == 0 ) || (next_line[0] != '@' && next_line[0] != '#' )))
            {
                if (next_line.Length > 0)
                {
                    valueBldr.AppendLine(next_line);
                }
                Instructions.Pop();
                next_line = Instructions.Peek();
            }
            string newValue = valueBldr.ToString();

            // Top level entity, or a child entity?
            if (entityName.IndexOf(">") < 0)
            {
                if ( entityName.IndexOf("#") > 0 )
                {
                    id = entityName.Substring(entityName.IndexOf("#") + 1);
                    entityName = entityName.Substring(0, entityName.IndexOf("#"));
                }

                ResourceFileEntity entity = File.GetEntity(entityName, id);
                if (entity == null) return false;

                entity.EntityName = newEntityName;
                entity.Value = newValue;
                entity.Changed = true;

                return true;
            }
            else
            {
                int seperatorIndex = entityName.IndexOf('>');
                string parentEntityName = entityName.Substring(0, seperatorIndex);
                string remainingEntityChain = entityName.Substring(seperatorIndex + 1);

                if (parentEntityName.IndexOf("#") > 0)
                {
                    id = parentEntityName.Substring(parentEntityName.IndexOf("#") + 1);
                    parentEntityName = parentEntityName.Substring(0, parentEntityName.IndexOf("#"));
                }

                var parentEntity = File.GetEntity(parentEntityName, id);
                parentEntity.Changed = true;

                return replace_entity_recursive(parentEntity, remainingEntityChain, newEntityName, newValue, 0);
            }
        }

        private bool replace_entity_recursive(ResourceFileEntity entity, string entityChain, string newEntityName, string newValue, int depth)
        {
            bool returnValue = false;

            int seperatorIndex = entityChain.IndexOf('>');
            if (seperatorIndex < 0)
            {
                var childEntity = entity.GetSubEntity(entityChain.Trim());
                childEntity.EntityName = newEntityName;
                childEntity.Value = newValue;
                childEntity.Changed = true;
                entity.Collapse(depth);
                entity.Changed = true;
            }
            else
            {
                string nextEntityName = entityChain.Substring(0, seperatorIndex);
                string remainingEntityChain = entityChain.Substring(seperatorIndex + 1);
                var nextEntity = entity.GetSubEntity(nextEntityName);
                returnValue = replace_entity_recursive(nextEntity, remainingEntityChain, newEntityName, newValue, depth + 1);
                entity.Collapse(depth);
                entity.Changed = true;
            }

            //entity.Collapse(depth);
            return returnValue;
        }
    }
}
