﻿using System.Linq;
using BrightData;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
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
        public void TestSvd()
        {
            var a = _cpu.CreateMatrix(256, 128, (x, y) => x * y).AsIndexable();
            var (floatMatrix, floatVector, floatMatrix1) = a.Svd();
            var reducedSize = 32.AsRange().ToList();

            var u = floatMatrix.GetNewMatrixFromRows(reducedSize);
            var s = _cpu.CreateDiagonalMatrix(floatVector.AsIndexable().Values.Take(reducedSize.Count).ToArray());
            var vt = floatMatrix1.GetNewMatrixFromColumns(reducedSize);
            var us = u.TransposeThisAndMultiply(s);
            var usvt = us.TransposeAndMultiply(vt);
            a.RowCount.Should().Be(usvt.RowCount);
            a.ColumnCount.Should().Be(usvt.ColumnCount);
        }
    }
}
