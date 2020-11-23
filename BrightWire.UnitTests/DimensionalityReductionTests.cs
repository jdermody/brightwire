using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightWire;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class DimensionalityReductionTests : NumericsBase
    {
        [Fact]
        public void TestRandomProjection()
        {
            var a = _cpu.CreateMatrix(256, 256, (x, y) => x * y).AsIndexable();
            var projector = _cpu.CreateRandomProjection(256, 32);
            var projections = projector.Compute(a);
            projections.ColumnCount.Should().Be(32);
            projections.RowCount.Should().Be(256);
        }

        [Fact]
        public void TestSVD()
        {
            var a = _cpu.CreateMatrix(256, 128, (x, y) => x * y).AsIndexable();
            var svd = a.Svd();
            var reducedSize = 32.AsRange().ToList();

            var u = svd.U.GetNewMatrixFromRows(reducedSize);
            var s = _cpu.CreateDiagonalMatrix(svd.S.AsIndexable().Values.Take(reducedSize.Count).ToArray());
            var vt = svd.VT.GetNewMatrixFromColumns(reducedSize);
            var us = u.TransposeThisAndMultiply(s);
            var usvt = us.TransposeAndMultiply(vt);
            a.RowCount.Should().Be(usvt.RowCount);
            a.ColumnCount.Should().Be(usvt.ColumnCount);
        }
    }
}
