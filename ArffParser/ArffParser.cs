using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArffParser
{
    public class ArffParser
    {
        private static readonly string ATTRIBUTE_TOKEN = "@ATTRIBUTE";
        private static readonly string RELATION_TOKEN = "@RELATION";
        private static readonly string DATA_TOKEN = "@DATA";
        private static readonly char COMMENT_TOKEN = '%';

        private string _curParseFileName;
        private int _curParseLineNumber;

        private enum ArffState
        {           
            HEADER, // contains description comments and @relation line
            ATTRIBUTE, // contains @attribute lines
            DATA // contains @data lines and data set
        }


        // sets up a state machine and parses according to the expected structure of ARFF
        // structure is:
        // @relation <identifier>
        //
        // @attribute <identifier>
        // <more attributes ...>
        //
        // @data
        // <data entries ...>
        public ArffRecord Parse(string filename)
        {
            _curParseFileName = filename;
            _curParseLineNumber = 0;

            string[] rawFileLines = System.IO.File.ReadAllLines(filename);
            string[] fileLines = PreprocessFileLines(rawFileLines); 

            ArffRecord arffRecord = new ArffRecord();
            arffRecord.FileName = filename;

            ArffState parseState = ArffState.HEADER;
            for(_curParseLineNumber = 1; _curParseLineNumber <= fileLines.Length; _curParseLineNumber++)
            {
                string fileLine = fileLines[_curParseLineNumber-1];
                if (fileLine.Length == 0 || fileLine[0] == COMMENT_TOKEN)
                    continue; // skip comment lines and new lines

                fileLine = fileLine.Split(COMMENT_TOKEN)[0].Trim(); // remove any trailing comments

                switch (parseState)
                {
                    case ArffState.HEADER:
                        if (fileLine.Contains(RELATION_TOKEN))
                        {
                            arffRecord.RelationName = TryParseRelation(fileLine);
                            parseState = ArffState.ATTRIBUTE;
                        }
                        else
                        {
                            // non empty line present in header state that did not contain relation token
                            throw new ArffParseException(_curParseFileName, _curParseLineNumber, "expected @relation declaration");
                        }
                        break;
                    case ArffState.ATTRIBUTE:
                        if (fileLine.Contains(ATTRIBUTE_TOKEN))
                        {
                            var attribute = TryParseAttribute(fileLine);
                            arffRecord.Attributes.Add(attribute);
                        }
                        else if (fileLine.Contains(DATA_TOKEN))
                        {
                            parseState = ArffState.DATA;
                        }
                        else
                        {
                            throw new ArffParseException(_curParseFileName, _curParseLineNumber, "expected @attribute declaration");
                        }
                        break;
                    case ArffState.DATA:
                        var data = TryParseData(fileLine, arffRecord.Attributes);
                        arffRecord.Data.Add(data);
                        break;
                }
            }

            return arffRecord;
        }


        // accepts a line with a @relation declaration and returns the parsed relation name
        private string TryParseRelation(string relationLine)
        {
            string[] tokens = relationLine.Split(' ');
            if (tokens.Length == 2)
            {
                return tokens[1];
                
            }
            else
            {
                throw new ArffParseException(_curParseFileName, _curParseLineNumber, "invalid @relation declaration");
            }
        }


        // parses a line with an @attribute declaration into an IAttribute object
        private IAttribute TryParseAttribute(string attrLine)        
        {
            string[] tokens = attrLine.Split(' ');

            if (tokens.Length < 3)
                throw new ArffParseException(_curParseFileName, _curParseLineNumber, "incomplete attribute declaration");

            if (tokens.Length > 3 && !IsClassAttributeLine(attrLine))
                throw new ArffParseException(_curParseFileName, _curParseLineNumber, "unrecognized tokens after attribute declaration");

            // correct number of tokens, continue parsing
            var attributeName = tokens[1];
            if (tokens[2].ToUpper() == AttributeType.NUMERIC.Value)
            {
                return new ContinuousAttribute(AttributeType.NUMERIC, attributeName);
            }
            else if (tokens[2].ToUpper() == AttributeType.DATE.Value)
            {
                return new ContinuousAttribute(AttributeType.DATE, attributeName);
            }
            else if (tokens[2].ToUpper() == AttributeType.STRING.Value)
            {
                return new ContinuousAttribute(AttributeType.STRING, attributeName);
            }
            else if (IsClassAttributeLine(attrLine))
            {
                return TryParseClassAttribute(attrLine, attributeName);
            }
            else
            {
                throw new ArffParseException
                    (
                        _curParseFileName,
                        _curParseLineNumber,
                        "attribute declaration does not match any recognized attribute types (NUMERIC, STRING, DATE, or class {...})"
                    );
            }
        }


        // parses a line of data against the provided attributes, returning a list of parsed, cast, and verified data entries
        private List<object> TryParseData(string dataLine, List<IAttribute> attributes)
        {
            string[] data = dataLine.Split(',');

            if (data.Length != attributes.Count)
                throw new ArffParseException
                (
                    _curParseFileName,
                    _curParseLineNumber,
                    $"expected {attributes.Count} data element{(attributes.Count > 1 ? "s" : "")}, {data.Length} {(data.Length == 1 ? "was" : "were")} present"
                );

            List<object> parsedData = new List<object>();
            // validate each data entry to ensure that it is an acceptable value for its corresponding attribute
            for (int i = 0; i < data.Length; i++)
            {
                var currAttr = attributes[i];
                if (currAttr.Type == AttributeType.DATE || currAttr.Type == AttributeType.STRING)
                {
                    parsedData.Add(data[i]);
                }
                else if (currAttr.Type == AttributeType.CLASS)
                {
                    if (((NominalAttribute)currAttr).Values.Contains(data[i]))
                    {
                        parsedData.Add(data[i]);
                    }
                    else
                    {                        
                        throw new ArffParseException(_curParseFileName, _curParseLineNumber, "invalid value for nominal attribute");
                    }
                }
                else if (currAttr.Type == AttributeType.NUMERIC)
                {
                    double numericValue;
                    if (Double.TryParse(data[i], out numericValue))
                    {
                        parsedData.Add(numericValue);
                    }
                    else
                    {
                        throw new ArffParseException(_curParseFileName, _curParseLineNumber, "invalid value for numeric attribute");
                    }
                }
            }
            // if all data entries were successfully parsed, return the 
            return parsedData;
        }


        private string[] PreprocessFileLines(string[] rawFileLines)
        {
            return rawFileLines
                .Select(line => line.Trim()) // remove leading and trailing whitespace
                .Select(line => line.Replace('\t', ' ')) // replace tabs with single spaces
                .Select(line => new Regex("\\s+").Replace(line, " ")) // condense multiple whitespaces into one                 
                .ToArray();
        }


        private bool IsClassAttributeLine(string line)
        {
            return line.Contains("{") && line.Contains("}");
        }


        private NominalAttribute TryParseClassAttribute(string attrLine, string attributeName)
        {
            // check for valid class
            if (attrLine.Last() != '}')
                throw new ArffParseException(_curParseFileName, _curParseLineNumber, "unrecognized tokens after class attribute declaration");

            var classValuesStart = attrLine.IndexOf('{');
            var classValuesEnd = attrLine.IndexOf('}');

            if (classValuesStart != -1 && classValuesEnd != -1 && classValuesStart < classValuesEnd)
            {
                var classValuesStr = attrLine.Substring(classValuesStart, classValuesEnd - classValuesStart)
                       .Replace("{", "")
                       .Replace("}", "")
                       .Replace(" ", ""); // remove all spaces

                var values = classValuesStr.Split(',').ToList();
                return new NominalAttribute(attributeName, values);
            }
            else
            {
                // attribute is not a valid class attribute and thus is not a valid type
                throw new ArffParseException(_curParseFileName, _curParseLineNumber, "invalid class attribute declaration");
            }
        }
    }
}
