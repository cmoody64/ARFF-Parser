using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArffParser
{
    public class ArffRecord
    {
        public ArffRecord()
        {
            Attributes = new List<IAttribute>();
            Data = new List<List<object>>();
        }

        public string FileName { get; set; }
        public string RelationName { get; set; }
        public List<IAttribute> Attributes { get; set; }
        public List<List<object>> Data { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"Relation {RelationName}: parsed from {FileName}\n");

            foreach(var attribute in Attributes)
            {
                stringBuilder.Append($"\t{attribute.ToString()}");
            }
            
            stringBuilder.Append($"Data: {Data.Count} items\n\n");
            return stringBuilder.ToString();
        }
    }
}
