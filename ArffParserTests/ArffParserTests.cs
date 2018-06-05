using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArffParser;

namespace SqlServerTest.Tests
{
    [TestClass()]
    public class ArffParserTests
    {
        [TestMethod()]
        public void ParseTest_SmallTestArff()
        {
            var arffParser = new ArffParser.ArffParser();
            ArffParser.ArffRecord record = arffParser.Parse("../../TestArffFiles/SmallTest.arff");

            Assert.IsTrue(record.RelationName == "cars");
            Assert.IsTrue(record.Attributes.Count == 5);

            // @ATTRIBUTE	buying       { vhigh, high, med, low }
            Assert.IsTrue(record.Attributes[0].Name == "buying");
            Assert.IsTrue(record.Attributes[0].Type == ArffParser.AttributeType.CLASS);
            Assert.IsTrue(((NominalAttribute)record.Attributes[0]).Values.Count == 4);
            Assert.IsTrue(((NominalAttribute)record.Attributes[0]).Values.Contains("vhigh"));
            Assert.IsTrue(((NominalAttribute)record.Attributes[0]).Values.Contains("high"));
            Assert.IsTrue(((NominalAttribute)record.Attributes[0]).Values.Contains("med"));
            Assert.IsTrue(((NominalAttribute)record.Attributes[0]).Values.Contains("low"));

            // @ATTRIBUTE price NUMERIC
            Assert.IsTrue(record.Attributes[1].Name == "price");
            Assert.IsTrue(record.Attributes[1].Type == ArffParser.AttributeType.NUMERIC);

            // @ATTRIBUTE miles numeric
            Assert.IsTrue(record.Attributes[2].Name == "miles");
            Assert.IsTrue(record.Attributes[2].Type == ArffParser.AttributeType.NUMERIC);

            // @ATTRIBUTE make STRING
            Assert.IsTrue(record.Attributes[3].Name == "make");
            Assert.IsTrue(record.Attributes[3].Type == ArffParser.AttributeType.STRING);

            // @ATTRIBUTE purchaseDate STRING
            Assert.IsTrue(record.Attributes[4].Name == "purchaseDate");
            Assert.IsTrue(record.Attributes[4].Type == ArffParser.AttributeType.DATE);

            // @DATA
            Assert.IsTrue(record.Data.Count == 3);
        }

        // tests that a comment in between the attribute name and attribute values results in an error
        [TestMethod()]
        public void ParseTest_Invalid1Arff()
        {
            var arffParser = new ArffParser.ArffParser();       
            Assert.ThrowsException<ArffParseException>(() => arffParser.Parse("../../TestArffFiles/Invalid1.arff"));
        }

        // tests that an incorrect ordering of declations (@relation, @data, @attribute) results in an error
        [TestMethod()]
        public void ParseTest_Invalid2Arff()
        {
            var arffParser = new ArffParser.ArffParser();
            Assert.ThrowsException<ArffParseException>(() => arffParser.Parse("../../TestArffFiles/Invalid2.arff"));
        }
        

        // tests that invalid nominal values (not belonging to nominal definition set) results in an error
        [TestMethod()]
        public void ParseTest_Invalid3Arff()
        {
            var arffParser = new ArffParser.ArffParser();
            Assert.ThrowsException<ArffParseException>(() => arffParser.Parse("../../TestArffFiles/Invalid3.arff"));
        }

        // tests that an incomplete data line results in an error
        [TestMethod()]
        public void ParseTest()
        {
            var arffParser = new ArffParser.ArffParser();
            Assert.ThrowsException<ArffParseException>(() => arffParser.Parse("../../TestArffFiles/Invalid4.arff"));
        }
    }
}
