using BrightWire;
using BrightWire.Models;
using BrightWire.TrainingData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class ClassificationSetTests
    {
        [TestMethod]
        public void TestTFIDF()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(stringTableBuilder);
            Assert.AreEqual(data.Count, 4);
            Assert.AreEqual(data.First().Data.Count, 3);
            var set = data.ConvertToWeightedIndexList(true);
            Assert.AreEqual(set.Count, 2);
            Assert.AreEqual(set.First().Data.Count, 4);

            var tfidf = set.TFIDF();
            Assert.AreEqual(tfidf.Count, 2);
            Assert.AreEqual(tfidf.First().Data.Count, 4);
        }
    }
}
