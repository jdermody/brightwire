using BrightWire;
using BrightWire.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class TreeBasedTests
    {
        [TestMethod]
        public void TestDecisionTree()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(stringTableBuilder).ConvertToSparseVectors(false).ConvertToTable();
            var model = data.TrainDecisionTree();
            var classifier = model.CreateClassifier();
            var testRows = data.GetRows(new[] { 0, data.RowCount - 1 });
            Assert.IsTrue(classifier.Classify(testRows[0]).First() == "china");
            Assert.IsTrue(classifier.Classify(testRows[1]).First() == "japan");
        }

        [TestMethod]
        public void TestRandomForest()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(stringTableBuilder).ConvertToSparseVectors(false).ConvertToTable();
            var model = data.TrainRandomForest();
            var classifier = model.CreateClassifier();
            var testRows = data.GetRows(new[] { 0, data.RowCount - 1 });
            Assert.IsTrue(classifier.Classify(testRows[0]).First() == "china");
            //Assert.IsTrue(classifier.Classify(testRows[1]).First() == "japan");
        }
    }
}
