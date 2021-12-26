using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder.Actions
{
    public class AddAction : IAction
    {
        public bool Apply(InstructionSet Instructions, ResourceFile File)
        {
            // Read the instruction line
            string instruction = Instructions.Pop().Trim();

            // Was this scoped within a single entity?
            string entityName = String.Empty;
            if (instruction.IndexOf(" ") > 0)
            {
                entityName = instruction.Substring(instruction.IndexOf(" ")).Trim();
            }

            // Get the lines to add
            var valueBldr = new StringBuilder();
            string next_line = Instructions.Peek();
            string newEntityName = next_line.Split('=')[0].Trim();
            while ((next_line != null) && ((next_line.Length == 0) || (next_line[0] != '@' && next_line[0] != '#')))
            {
                if (next_line.Length > 0)
                {
                    valueBldr.AppendLine(next_line);
                }
                Instructions.Pop();
                next_line = Instructions.Peek();
            }
            string newValue = valueBldr.ToString();

            // Create the new entity
            var newEntity = new ResourceFileEntity
            {
                EntityName = newEntityName,
                Value = newValue,
                Type = ResourceFileEntityType.Entity
            };

            // Top level entity?
            if ( String.IsNullOrEmpty(entityName))
            {
                File.Entities.Add(newEntity);
            }
            else if (entityName.IndexOf(">") < 0)
            {
                var parentEntity = File.GetEntity(entityName);
                parentEntity.Parse();
                parentEntity.SubEntities.Add(newEntity);
                parentEntity.Collapse(0);
            }
            else
            {
                int seperatorIndex = entityName.IndexOf('>');
                string parentEntityName = entityName.Substring(0, seperatorIndex);
                string remainingEntityChain = entityName.Substring(seperatorIndex + 1);

                var parentEntity = File.GetEntity(parentEntityName);

                add_entity_recursive(parentEntity, remainingEntityChain, newEntity, 0);
                if (parentEntity.SubEntities != null)
                    parentEntity.Collapse(0);
                
            }

            return true;
        }

        private bool add_entity_recursive(ResourceFileEntity entity, string entityChain, ResourceFileEntity newEntity, int depth)
        {
            bool returnValue = false;

            int seperatorIndex = entityChain.IndexOf('>');
            if (seperatorIndex < 0)
            {
                var childEntity = entity.GetSubEntity(entityChain.Trim());
                childEntity.Parse();
                childEntity.SubEntities.Add(newEntity);
                childEntity.Collapse(depth);
            }
            else
            {
                string nextEntityName = entityChain.Substring(0, seperatorIndex);
                string remainingEntityChain = entityChain.Substring(seperatorIndex + 1);
                var nextEntity = entity.GetSubEntity(nextEntityName);
                returnValue = add_entity_recursive(nextEntity, remainingEntityChain, newEntity, depth + 1);
                entity.Collapse(depth);
            }

            //entity.Collapse(depth);
            return returnValue;
        }
    }
}
