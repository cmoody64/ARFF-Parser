using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArffParser
{
    public interface IAttribute
    {
        AttributeType Type { get; }
        string Name { get; }
    }

    public class ContinuousAttribute : IAttribute
    {
        public ContinuousAttribute(AttributeType type, string name)
        {
            Type = type;
            Name = name;
        }
        public AttributeType Type { get; }
        public string Name { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"Attribute {Name}: {Type}\n");
            return stringBuilder.ToString();
        }
    }

    public class NominalAttribute : IAttribute
    {
        public NominalAttribute(string name, List<string> values)
        {
            Name = name;
            Values = values.AsReadOnly();
        }
        public AttributeType Type { get { return AttributeType.CLASS; } }
        public string Name { get; }
        public IReadOnlyList<string> Values { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"Attribute {Name}:");
            stringBuilder.Append(" { ");
            foreach(var value in Values)
            {
                stringBuilder.Append($"{value} ");
            }
            stringBuilder.Append("}\n");
            return stringBuilder.ToString();
        }
    }

    public class AttributeType
    {
        private AttributeType(string value)
        {
            Value = value;
        }
        public string Value;

        public static AttributeType CLASS { get { return new AttributeType("CLASS"); } }
        public static AttributeType NUMERIC { get { return new AttributeType("NUMERIC"); } }
        public static AttributeType DATE { get { return new AttributeType("DATE"); } }
        public static AttributeType STRING { get { return new AttributeType("STRING"); } }

        public static bool operator== (AttributeType a, AttributeType b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(AttributeType a, AttributeType b)
        {
            return a.Value != b.Value;
        }

        public override bool Equals(object obj)
        {
            var type = obj as AttributeType;
            return type != null &&
                   Value == type.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
    }
}
