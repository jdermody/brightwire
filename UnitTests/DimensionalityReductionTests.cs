using BrightWire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class DimensionalityReductionTests
    {
        static ILinearAlgebraProvider _lap;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _lap = Provider.CreateCPULinearAlgebra();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _lap.Dispose();
        }

        [TestMethod]
        public void TestRandomProjection()
        {
            var a = _lap.Create(256, 256, (x, y) => x * y).AsIndexable();
            var projector = _lap.CreateRandomProjection(256, 32);
            var projections = projector.Compute(a);
            Assert.IsTrue(projections.ColumnCount == 32);
            Assert.IsTrue(projections.RowCount == 256);
        }

        [TestMethod]
        public void TestSVD()
        {
            var a = _lap.Create(256, 256, (x, y) => x * y).AsIndexable();
            var svd = a.Svd();
            var reducedSize = Enumerable.Range(0, 32).ToList();

            var u = svd.Item1.GetNewMatrixFromRows(reducedSize);
            var s = _lap.CreateDiagonal(svd.Item2.AsIndexable().Values.Take(reducedSize.Count).ToArray());
            var vt = svd.Item3.GetNewMatrixFromColumns(reducedSize);
            var us = u.TransposeThisAndMultiply(s);
            var usvt = us.TransposeAndMultiply(vt);
        }
    }
}
