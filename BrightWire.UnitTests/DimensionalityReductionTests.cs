using System.Linq;
using BrightData;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class DimensionalityReductionTests : CpuBase
    {
        [Fact]
        public void TestRandomProjection()
        {
            var lap = _context.LinearAlgebraProvider2;
            var a = lap.CreateMatrix(256, 256, (x, y) => x * y);
            var projector = lap.CreateRandomProjection(256, 32);
            var projections = projector.Compute(a);
            projections.ColumnCount.Should().Be(32);
            projections.RowCount.Should().Be(256);
        }

        [Fact]
        public void TestSvd()
        {
            var lap = _context.LinearAlgebraProvider2;
            var a = lap.CreateMatrix(256, 128, (x, y) => x * y);
            var (floatMatrix, floatVector, floatMatrix1) = a.Svd();
            var reducedSize = 32.AsRange().ToList();

            var u = floatMatrix.GetNewMatrixFromRows(reducedSize);
            var s = lap.CreateDiagonalMatrix(floatVector.Segment.Values.Take(reducedSize.Count).ToArray());
            var vt = floatMatrix1.GetNewMatrixFromColumns(reducedSize);
            var us = u.TransposeThisAndMultiply(s);
            var usvt = us.TransposeAndMultiply(vt);
            a.RowCount.Should().Be(usvt.RowCount);
            a.ColumnCount.Should().Be(usvt.ColumnCount);
        }
    }
}
