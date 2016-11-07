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
    public class UnsupervisedTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = Provider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void TestKMeans()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(stringTableBuilder).ConvertToSet(false).Encode(true).ToDictionary(d => _lap.Create(d.Data), d => d.Classification);
            var clusters = data.Select(d => d.Key).ToList().KMeans(2);
            var clusterLabels = clusters.Select(d => d.Select(d2 => data[d2]).ToArray()).ToList();
        }

        [TestMethod]
        public void TestNNMF()
        {
            var stringTableBuilder = new StringTableBuilder();
            var data = NaiveBayesTests.GetSimpleChineseSet(stringTableBuilder).ConvertToSet(false).Encode(true).ToDictionary(d => _lap.Create(d.Data), d => d.Classification);
            var clusters = data.Select(d => d.Key).ToList().NNMF(_lap, 2);
            var clusterLabels = clusters.Select(d => d.Select(d2 => data[d2]).ToArray()).ToList();
        }
    }
}
