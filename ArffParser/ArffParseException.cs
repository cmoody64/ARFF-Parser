using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArffParser
{
    public class ArffParseException: Exception
    {
        public ArffParseException(string filename, int lineNumber, string reason)
            : base($"Error parsing {filename} at line {lineNumber}: {reason}")
        {
        }
    }
}
