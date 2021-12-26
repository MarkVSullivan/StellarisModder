using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public static class ResourceFileEntityParser
    {
        public static List<ResourceFileEntity> Parse(string source)
        {
            var parsed = new List<ResourceFileEntity>();
            ResourceFileEntity entity = null;
            StringBuilder valueBuilder = new StringBuilder();
            int entity_depth = 0;

            string[] split = source.Split('\r');
            if (split.Length == 1)
                split = source.Split('\n');
            foreach (string thisLine in split)
            {
                // Trim the line
                string trimmed = thisLine.Trim();

                // Empty line (ignore)
                if (trimmed.Length == 0)
                    continue;

                // Comment line (single line)
                if ((trimmed[0] == '#') && (entity == null))
                {
                    // Create and add a simple comment entity
                    ResourceFileEntity commentEntity = new ResourceFileEntity
                    {
                        Type = ResourceFileEntityType.Comments,
                        Value = trimmed,
                        EntityName = trimmed
                    };
                    parsed.Add(commentEntity);
                    continue;
                }

                // See if there are brackets in this line
                int count = NestingCount(trimmed);

                // Simple definition line (not in an entity we are building)
                if ((entity == null) && (count == 0))
                {
                    // Create a simple definition entity 
                    ResourceFileEntity definitionEntity = new ResourceFileEntity
                    {
                        Type = ResourceFileEntityType.Definition,
                        Value = trimmed,
                        EntityName = trimmed
                    };

                    // Is there an entity name on this row (likely)
                    string defName = entity_name(trimmed);
                    if (!String.IsNullOrEmpty(defName))
                        definitionEntity.EntityName = defName;

                    // Add the definition entity
                    parsed.Add(definitionEntity);
                    continue;
                }

                // Starting a new entity?
                if ((entity == null) && (count > 0))
                {
                    // Create the new entity 
                    entity = new ResourceFileEntity
                    {
                        Type = ResourceFileEntityType.Entity,
                        EntityName = trimmed
                    };

                    // Is there an entity name on this row (likely)
                    string defName = entity_name(trimmed);
                    if (!String.IsNullOrEmpty(defName))
                        entity.EntityName = defName;

                    // Start building this value
                    valueBuilder.Clear();
                    valueBuilder.Append(trimmed + Environment.NewLine);

                    // Keep track of depth while parsing the entity
                    entity_depth = count;
                    continue;
                }

                // In the middle of an entity already
                if (entity != null)
                {
                    // determine depth for indent
                    int indent_depth = entity_depth;
                    if ((count == -1) && (trimmed.Length == 1))
                        indent_depth = entity_depth - 1;

                    // Add this line to the builder
                    valueBuilder.Append(indent(indent_depth) + trimmed + Environment.NewLine);

                    // Add the depth this line added/subtracted to the entity depth
                    entity_depth += count;

                    // Are we done with this entity? ( i.e., depth is back to zero?)
                    if (entity_depth == 0)
                    {
                        entity.Value = valueBuilder.ToString();
                        parsed.Add(entity);
                        entity = null;
                    }
                }
            }

            // Shouldn't still have an entity here really.

            return parsed;
        }

        private static string indent(int depth)
        {
            if (depth < 0)
                return String.Empty;

            switch (depth)
            {
                case 0:
                    return String.Empty;

                case 1:
                    return "\t";

                case 2:
                    return "\t\t";

                case 3:
                    return "\t\t\t";

                case 4:
                    return "\t\t\t\t";

                case 5:
                    return "\t\t\t\t\t";

                case 6:
                    return "\t\t\t\t\t\t";

                case 7:
                    return "\t\t\t\t\t\t\t";

                default:
                    return "\t" + indent(depth - 1);

            }
        }

        private static string entity_name(string line)
        {
            if (line.IndexOf("=") < 0)
                return null;

            return line.Split('=')[0].Trim();
        }

        public static int NestingCount(string line)
        {
            if ((line.IndexOf('{') < 0) && (line.IndexOf('}') < 0))
                return 0;

            int left_brackets = numberOf(line, '{');
            int right_brackets = numberOf(line, '}');

            return left_brackets - right_brackets;
        }

        private static int numberOf(string line, char character)
        {
            int returnValue = 0;
            int startIndex = 0;
            int nextIndex = line.IndexOf(character); ;
            while (nextIndex >= 0)
            {
                returnValue++;
                startIndex = nextIndex + 1;
                if (startIndex > line.Length)
                    break;
                nextIndex = line.IndexOf(character, startIndex);
            }
            return returnValue;
        }
    }
}
