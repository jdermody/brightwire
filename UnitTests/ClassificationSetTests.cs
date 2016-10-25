using BrightWire.Helper;
using BrightWire.Models;
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
            var bag = new ClassificationBag {
                Classifications = new[] {
                    Tuple.Create(new[] { "Chinese", "Beijing", "Chinese" }, true),
                    Tuple.Create(new[] { "Chinese", "Chinese", "Shanghai" }, true),
                    Tuple.Create(new[] { "Chinese", "Macao" }, true),
                    Tuple.Create(new[] { "Tokyo", "Japan", "Chinese" }, false),
                }.Select(d => new ClassificationBag.Classification {
                    Name = d.Item2 ? "china" : "japan",
                    Data = d.Item1.Select(s => stringTableBuilder.GetIndex(s)).ToArray()
                }).ToArray()
            };
            Assert.AreEqual(bag.Classifications.Length, 4);
            Assert.AreEqual(bag.Classifications[0].Data.Length, 3);
            var set = bag.ConvertToSet(true);
            Assert.AreEqual(set.Classifications.Length, 2);
            Assert.AreEqual(set.Classifications[0].Data.Length, 4);

            var tfidf = set.TFIDF();
            Assert.AreEqual(tfidf.Classifications.Length, 2);
            Assert.AreEqual(tfidf.Classifications[0].Data.Length, 4);
        }
    }
}
