using System;
using BrightWire;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests2
{
    [TestClass]
    public class CSVParserTests
    {
        [TestMethod]
        public void EmptyCSV()
        {
            var csv = "".ParseCSV();
        }

        [TestMethod]
        public void NullCSV()
        {
            var csv = Provider.ParseCSV((string)null);
        }

        [TestMethod]
        public void TestSimple()
        {
            var table = @"
                123,1.0,a
                234,0.5,b
                0,0,c"
                .ParseCSV()
            ;
        }

        [TestMethod]
        public void TestSimpleWithHeader()
        {
            var table = @"
                a,b,c
                123,1.0,a
                234,0.5,b
                0,0,c"
                .ParseCSV()
            ;
        }
    }
}
