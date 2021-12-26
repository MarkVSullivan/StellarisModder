using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder
{
    public class ResourceFileEntity
    {
        public string EntityName { get; set; }

        private string entityValue;
        public string Value 
        { 
            get { return entityValue;  }
            set
            {
                SubEntities = null;
                entityValue = value;
            }
        }

        public ResourceFileEntityType Type { get; set; }

        public List<ResourceFileEntity> SubEntities { get; private set;  }

        public void Parse()
        {
            string[] split = Value.Split('\r');
            StringBuilder builder = new StringBuilder();
            for ( int i = 1; i < split.Length -1; i++ )
            {
                builder.AppendLine(split[i].Replace("\n", ""));
            }

            SubEntities = ResourceFileEntityParser.Parse(builder.ToString());
        }

        public void Collapse( int depth )
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(indent(depth) + EntityName + " = {");

            // Write each entity
            foreach (var entity in SubEntities)
            {
                // Write the value of the entity
                if (entity.Value.IndexOf("\r") < 0)
                {
                    builder.AppendLine(indent(depth + 1) + entity.Value);
                }
                else
                {
                    // Split into lines, so we can indent correctly
                    string[] split = entity.Value.Split('\r');
                    int entity_depth = depth + 1;

                    foreach (string thisSplit in split)
                    {
                        // Trim the line
                        string trimmed = thisSplit.Trim();

                        // See if there are brackets in this line
                        int count = ResourceFileEntityParser.NestingCount(trimmed);

                        // determine depth for indent
                        int indent_depth = entity_depth;
                        if ((count == -1) && (trimmed.Length == 1))
                            indent_depth = entity_depth - 1;

                        // Add this line to the builder
                        builder.Append(indent(indent_depth) + trimmed + Environment.NewLine);

                        // Add the depth this line added/subtracted to the entity depth
                        entity_depth += count;
                    }
                }
            }

            builder.AppendLine(indent(depth) + "}");

            Value = builder.ToString();

            SubEntities = null;
        }

        public ResourceFileEntity GetSubEntity(string EntityName)
        {
            if (SubEntities == null)
                Parse();

            foreach (ResourceFileEntity entity in SubEntities)
            {
                if ((entity.Type != ResourceFileEntityType.Comments) && (entity.EntityName.Equals(EntityName, StringComparison.OrdinalIgnoreCase)))
                {
                    return entity;
                }
            }

            return null;
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
    }

    public enum ResourceFileEntityType : byte
    {
        Entity,

        Comments,

        Definition
    }
}
