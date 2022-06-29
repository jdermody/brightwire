using System;
using System.Linq;
using BrightData.Helper;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class MatrixTests : CudaBase
    {
        //static IIndexableFloatMatrix Apply(ILinearAlgebraProvider lap, IIndexableFloatMatrix a, IIndexableFloatMatrix b, Func<IFloatMatrix, IFloatMatrix, IFloatMatrix> func)
        //{
        //    using var otherA = lap.CreateMatrix(a);
        //    using var otherB = lap.CreateMatrix(b);
        //    using var otherC = func(otherA, otherB);
        //    return otherC.AsIndexable();
        //}

        //static IIndexableFloatMatrix Apply(ILinearAlgebraProvider lap, IIndexableFloatMatrix a, IIndexableFloatMatrix b, Action<IFloatMatrix, IFloatMatrix> func)
        //{
        //    using var otherA = lap.CreateMatrix(a);
        //    using var otherB = lap.CreateMatrix(b);
        //    func(otherA, otherB);
        //    return otherA.AsIndexable();
        //}

        //static IIndexableFloatMatrix Apply(ILinearAlgebraProvider lap, IIndexableFloatMatrix a, IIndexableFloatVector b, Action<IFloatMatrix, IFloatVector> func)
        //{
        //    using var otherA = lap.CreateMatrix(a);
        //    using var otherB = lap.CreateVector(b);
        //    func(otherA, otherB);
        //    return otherA.AsIndexable();
        //}

        //static IIndexableFloatMatrix Apply(ILinearAlgebraProvider lap, IIndexableFloatMatrix a, Func<IFloatMatrix, IFloatMatrix> func)
        //{
        //    using var otherA = lap.CreateMatrix(a);
        //    using var otherB = func(otherA);
        //    return otherB.AsIndexable();
        //}

        //static IIndexableFloatMatrix Apply(ILinearAlgebraProvider lap, IIndexableFloatMatrix a, Action<IFloatMatrix> func)
        //{
        //    using var otherA = lap.CreateMatrix(a);
        //    func(otherA);
        //    return otherA.AsIndexable();
        //}

        //static IIndexableFloatVector Apply(ILinearAlgebraProvider lap, IIndexableFloatMatrix a, Func<IFloatMatrix, IFloatVector> func)
        //{
        //    using var otherA = lap.CreateMatrix(a);
        //    using var otherRow = func(otherA);
        //    return otherRow.AsIndexable();
        //}

        //void CheckMatrixMultiplication(uint rowsA, uint columnsArowsB, uint columnsB)
        //{
        //    var rand = new Random(1);
        //    var a = _cpu.CreateMatrix(rowsA, columnsArowsB, (_, _) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var b = _cpu.CreateMatrix(columnsArowsB, columnsB, (_, _) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var cpuResults = a.Multiply(b);

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.Multiply(b));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.Multiply(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void TestMatrixCreationFromRows()
        //{
        //    var values = new[] {
        //        Enumerable.Range(0, 10).Select(v => (float)v).ToArray(),
        //        Enumerable.Range(0, 10).Select(v => (float)v * 2).ToArray(),
        //        Enumerable.Range(0, 10).Select(v => (float)v * 3).ToArray(),
        //    };
        //    var cpuRowList = values.Select(v => _cpu.CreateVector(v)).ToList();
        //    var cpuMatrix = _cpu.CreateMatrixFromRows(cpuRowList);

        //    var gpuRowList = values.Select(v => _cuda.CreateVector(v)).ToList();
        //    using (var gpuMatrix = _cuda.CreateMatrixFromRows(gpuRowList)) {
        //        FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable()).Should().BeTrue();
        //    }
        //    gpuRowList.ForEach(v => v.Dispose());

        //    var simpleRowList = values.Select(v => _simple.CreateVector(v)).ToList();
        //    using (var simpleMatrix = _simple.CreateMatrixFromRows(simpleRowList)) {
        //        FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), simpleMatrix.AsIndexable()).Should().BeTrue();
        //    }
        //    simpleRowList.ForEach(v => v.Dispose());
        //}

        //[Fact]
        //public void TestMatrixCreationFromColumns()
        //{
        //    var values = new[] {
        //        Enumerable.Range(0, 10).Select(v => (float)v).ToArray(),
        //        Enumerable.Range(0, 10).Select(v => (float)v * 2).ToArray(),
        //        Enumerable.Range(0, 10).Select(v => (float)v * 3).ToArray(),
        //    };
        //    var cpuRowList = values.Select(v => _cpu.CreateVector(v)).ToList();
        //    var cpuMatrix = _cpu.CreateMatrixFromColumns(cpuRowList);

        //    var gpuRowList = values.Select(v => _cuda.CreateVector(v)).ToList();
        //    using (var gpuMatrix = _cuda.CreateMatrixFromColumns(gpuRowList)) {
        //        FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable()).Should().BeTrue();
        //    }
        //    gpuRowList.ForEach(v => v.Dispose());

        //    var simpleRowList = values.Select(v => _simple.CreateVector(v)).ToList();
        //    using (var simpleMatrix = _simple.CreateMatrixFromColumns(simpleRowList)) {
        //        FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), simpleMatrix.AsIndexable()).Should().BeTrue();
        //    }
        //    simpleRowList.ForEach(v => v.Dispose());
        //}

        //[Fact]
        //public void MatrixMultiplication()
        //{
        //    CheckMatrixMultiplication(5, 2, 5);
        //}

        //[Fact]
        //public void MatrixMultiplication2()
        //{
        //    CheckMatrixMultiplication(500, 200, 500);
        //}

        //[Fact]
        //public void MatrixMultiplication3()
        //{
        //    CheckMatrixMultiplication(5, 5, 5);
        //}

        //[Fact]
        //public void MatrixMultiplication4()
        //{
        //    CheckMatrixMultiplication(5, 10, 5);
        //}

        //[Fact]
        //public void MatrixMultiplication5()
        //{
        //    CheckMatrixMultiplication(5, 10, 2);
        //}

        //[Fact]
        //public void MatrixMultiplication6()
        //{
        //    CheckMatrixMultiplication(50, 10, 2);
        //}

        //[Fact]
        //public void MatrixMultiplication7()
        //{
        //    CheckMatrixMultiplication(2, 10, 20);
        //}

        //[Fact]
        //public void MatrixMultiplication8()
        //{
        //    CheckMatrixMultiplication(20, 1, 19);
        //}

        //void Transpose(uint rows, uint columns)
        //{
        //    var a = _cpu.CreateMatrix(rows, columns, (j, k) => k).AsIndexable();
        //    var aT = a.Transpose();

        //    var gpuResults = Apply(_cuda, a, a => a.Transpose());
        //    FloatMath.AreApproximatelyEqual(gpuResults, aT.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.Transpose());
        //    FloatMath.AreApproximatelyEqual(simpleResults, aT.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixTranspose()
        //{
        //    Transpose(2, 5);
        //}

        //[Fact]
        //public void MatrixTranspose2()
        //{
        //    Transpose(5, 2);
        //}

        //[Fact]
        //public void MatrixTranspose3()
        //{
        //    Transpose(500, 2);
        //}

        //[Fact]
        //public void MatrixTranspose4()
        //{
        //    Transpose(2, 500);
        //}

        //[Fact]
        //public void MatrixTranspose5()
        //{
        //    Transpose(20, 20);
        //}

        //[Fact]
        //public void MatrixTranspose6()
        //{
        //    Transpose(500, 500);
        //}

        //[Fact]
        //public void MatrixTransposeAndMultiplication()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var b = _cpu.CreateMatrix(3, 5, (j, k) => j).AsIndexable();
        //    var cpuResults = a.TransposeAndMultiply(b);

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.TransposeAndMultiply(b));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.TransposeAndMultiply(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixTransposeAndMultiplication2()
        //{
        //    var a = _cpu.CreateMatrix(2, 6, (j, k) => k).AsIndexable();
        //    var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();
        //    var cpuResults = a.TransposeThisAndMultiply(b);

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.TransposeThisAndMultiply(b));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.TransposeThisAndMultiply(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixAdd()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();
        //    var cpuResults = a.Add(b);

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.Add(b));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.Add(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSubtract()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();

        //    IIndexableFloatMatrix gpuResults, gpuResults2;
        //    using (var gpuA = _cuda.CreateMatrix(a))
        //    using (var gpuB = _cuda.CreateMatrix(b)) {
        //        var aStr = gpuA.ToString();
        //        var bStr = gpuB.ToString();
        //        using (var gpuC = gpuA.Subtract(gpuB))
        //        using (var gpuD = gpuB.Subtract(gpuA)) {
        //            gpuResults = gpuC.AsIndexable();
        //            gpuResults2 = gpuD.AsIndexable();
        //        }

        //        aStr.Should().BeEquivalentTo(gpuA.ToString());
        //        bStr.Should().BeEquivalentTo(gpuB.ToString());
        //    }

        //    IIndexableFloatMatrix simpleResults, simpleResults2;
        //    using (var simpleA = _simple.CreateMatrix(a))
        //    using (var simpleB = _simple.CreateMatrix(b)) {
        //        var aStr = simpleA.ToString();
        //        var bStr = simpleB.ToString();
        //        using (var simpleC = simpleA.Subtract(simpleB))
        //        using (var simpleD = simpleB.Subtract(simpleA)) {
        //            simpleResults = simpleC.AsIndexable();
        //            simpleResults2 = simpleD.AsIndexable();
        //        }

        //        aStr.Should().BeEquivalentTo(simpleA.ToString());
        //        bStr.Should().BeEquivalentTo(simpleB.ToString());
        //    }

        //    var cpuResults = a.Subtract(b);
        //    var cpuResults2 = b.Subtract(a);
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(gpuResults2, cpuResults2.AsIndexable()).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults2, cpuResults2.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixPointwiseMultiply()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();
        //    var cpuResults = a.PointwiseMultiply(b);

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.PointwiseMultiply(b));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.PointwiseMultiply(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixPointwiseDivide()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k + 1).AsIndexable();
        //    var b = _cpu.CreateMatrix(2, 5, (j, k) => j + 1).AsIndexable();
        //    var cpuResults = a.PointwiseDivide(b);

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.PointwiseDivide(b));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.PointwiseDivide(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSqrt()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k + 1).AsIndexable();
        //    a[0, 0] = -1e-8f;
        //    var cpuResults = a.Sqrt();

        //    var gpuResults = Apply(_cuda, a, a => a.Sqrt());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.Sqrt());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixMultiplyScalar()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    const float SCALAR = 2.5f;

        //    var gpuResults = Apply(_cuda, a, a => a.Multiply(SCALAR));
        //    var simpleResults = Apply(_simple, a, a => a.Multiply(SCALAR));

        //    a.Multiply(SCALAR);
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixColumn()
        //{
        //    const int INDEX = 7;
        //    var a = _cpu.CreateMatrix(13, 17, (j, k) => (j + 1) * (k + 1)).AsIndexable();
        //    var row = a.Column(INDEX).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.Column(INDEX));
        //    FloatMath.AreApproximatelyEqual(gpuResults, row).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.Column(INDEX));
        //    FloatMath.AreApproximatelyEqual(simpleResults, row).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixRow()
        //{
        //    const int INDEX = 11;
        //    var a = _cpu.CreateMatrix(20, 50, (j, k) => k * j).AsIndexable();
        //    var row = a.Row(INDEX).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.Row(INDEX));
        //    FloatMath.AreApproximatelyEqual(gpuResults, row).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.Row(INDEX));
        //    FloatMath.AreApproximatelyEqual(simpleResults, row).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixRowSums()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var rowSums = a.RowSums().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.RowSums());
        //    FloatMath.AreApproximatelyEqual(gpuResults, rowSums).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.RowSums());
        //    FloatMath.AreApproximatelyEqual(simpleResults, rowSums).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixColumnSums()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var colSums = a.ColumnSums().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.ColumnSums());
        //    FloatMath.AreApproximatelyEqual(gpuResults, colSums).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.ColumnSums());
        //    FloatMath.AreApproximatelyEqual(simpleResults, colSums).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixAddInPlace()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.AddInPlace(b, 1.5f, 2.5f));
        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.AddInPlace(b, 1.5f, 2.5f));

        //    a.AddInPlace(b, 1.5f, 2.5f);
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSubtractInPlace()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
        //    var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.SubtractInPlace(b, 1.5f, 2.5f));
        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.SubtractInPlace(b, 1.5f, 2.5f));

        //    a.SubtractInPlace(b, 1.5f, 2.5f);
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixAddToEachRow()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j).AsIndexable();
        //    var b = _cpu.CreateVector(5, (i) => i).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.AddToEachRow(b));
        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.AddToEachRow(b));

        //    a.AddToEachRow(b);
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixAddToEachColumn()
        //{
        //    var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j).AsIndexable();
        //    var b = _cpu.CreateVector(2, (i) => i + 5).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.AddToEachColumn(b));
        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.AddToEachColumn(b));

        //    a.AddToEachColumn(b);
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSigmoidActivation()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.SigmoidActivation().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, (a) => a.SigmoidActivation());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, (a) => a.SigmoidActivation());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSigmoidDerivative()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.SigmoidDerivative().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, (a) => a.SigmoidDerivative());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, (a) => a.SigmoidDerivative());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixTanhActivation()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.TanhActivation().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.TanhActivation());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, (a) => a.TanhActivation());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixTanhDerivative()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.TanhDerivative().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.TanhDerivative());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_cuda, a, a => a.TanhDerivative());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixReluActivation()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.ReluActivation().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.ReluActivation());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_cuda, a, a => a.ReluActivation());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixReluDerivative()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.ReluDerivative().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.ReluDerivative());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_cuda, a, a => a.ReluDerivative());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixLeakyReluActivation()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.LeakyReluActivation().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.LeakyReluActivation());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_cuda, a, a => a.LeakyReluActivation());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixLeakyReluDerivative()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.LeakyReluDerivative().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.LeakyReluDerivative());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_cuda, a, a => a.LeakyReluDerivative());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSoftmaxActivation()
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
        //    var cpuResults = a.SoftmaxActivation().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.SoftmaxActivation());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_cuda, a, a => a.SoftmaxActivation());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixNewMatrixFromRows()
        //{
        //    var rows = new uint[] { 7, 8, 9 };
        //    var a = _cpu.CreateMatrix(13, 17, (j, k) => (k + 1) * (j + 1)).AsIndexable();
        //    var cpuResults = a.GetNewMatrixFromRows(rows).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.GetNewMatrixFromRows(rows));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.GetNewMatrixFromRows(rows));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixNewMatrixFromColumns()
        //{
        //    var cols = new uint[] { 1, 2, 9 };
        //    var a = _cpu.CreateMatrix(12, 13, (j, k) => (k + 1) * (j + 1)).AsIndexable();
        //    var cpuResults = a.GetNewMatrixFromColumns(cols).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.GetNewMatrixFromColumns(cols));
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.GetNewMatrixFromColumns(cols));
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixClearRows()
        //{
        //    var rows = new uint[] { 1, 2, 9 };
        //    var a = _cpu.CreateMatrix(13, 12, (j, k) => k + 1).AsIndexable();
            
        //    var gpuResults = Apply(_cuda, a, a => a.ClearRows(rows));
        //    var simpleResults = Apply(_simple, a, a => a.ClearRows(rows));

        //    a.ClearRows(rows);
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixClearColumns()
        //{
        //    var cols = new uint[] { 1, 2, 7 };
        //    var a = _cpu.CreateMatrix(18, 13, (j, k) => k + 1).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.ClearColumns(cols));
        //    var simpleResults = Apply(_simple, a, a => a.ClearColumns(cols));

        //    a.ClearColumns(cols);
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixClear()
        //{
        //    var a = _cpu.CreateMatrix(15, 23, (j, k) => k + 1).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.Clear());
        //    var simpleResults = Apply(_simple, a, a => a.Clear());

        //    a.Clear();
        //    FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults, a).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixClone()
        //{
        //    var a = _cpu.CreateMatrix(12, 7, (j, k) => k + 1).AsIndexable();
        //    var cpuResults = a.Clone().AsIndexable();
        //    FloatMath.AreApproximatelyEqual(a, cpuResults);

        //    var gpuResults = Apply(_cuda, a, a => a.Clone());
        //    FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.Clone());
        //    FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixReadWrite()
        //{
        //    var a = _cpu.CreateMatrix(7, 20, (x, y) => x * 10 + y).AsIndexable();

        //    // test Numerics -> Numerics serialisation
        //    var serialised = a.Data;
        //    var b = _cpu.CreateMatrix(serialised);
        //    FloatMath.AreApproximatelyEqual(a.AsIndexable(), b.AsIndexable()).Should().BeTrue();

        //    // test Numerics -> Cuda serialisation
        //    using var c = _cuda.CreateMatrix(serialised);
        //    FloatMath.AreApproximatelyEqual(a.AsIndexable(), c.AsIndexable()).Should().BeTrue();

        //    // test Cuda -> Cuda serialisation
        //    var serialised2 = c.Data;
        //    using (var d = _cuda.CreateMatrix(serialised2))
        //        FloatMath.AreApproximatelyEqual(a.AsIndexable(), d.AsIndexable()).Should().BeTrue();

        //    // test Cuda -> Numerics serialisation
        //    var e = _cpu.CreateMatrix(c.Data);
        //    FloatMath.AreApproximatelyEqual(a.AsIndexable(), e.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixConcatColumns()
        //{
        //    var rand = new Random();
        //    var a = _cpu.CreateMatrix(4000, 300, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var b = _cpu.CreateMatrix(200, 300, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var cpuResults = a.ConcatColumns(b).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.ConcatColumns(b));
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.ConcatColumns(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, gpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixConcatRows()
        //{
        //    var rand = new Random();
        //    var a = _cpu.CreateMatrix(300, 4000, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var b = _cpu.CreateMatrix(300, 200, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var cpuResults = a.ConcatRows(b).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.ConcatRows(b));
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.ConcatRows(b));
        //    FloatMath.AreApproximatelyEqual(simpleResults, gpuResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSplitColumns()
        //{
        //    const int POSITION = 2000;
        //    var rand = new Random();
        //    var a = _cpu.CreateMatrix(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var (top, bottom) = a.SplitAtRow(POSITION);

        //    IIndexableFloatMatrix gpuResults1, gpuResults2;
        //    using (var gpuA = _cuda.CreateMatrix(a)) {
        //        var (top2, bottom2) = gpuA.SplitAtRow(POSITION);
        //        using var m1 = top2;
        //        using var m2 = bottom2;
        //        gpuResults1 = m1.AsIndexable();
        //        gpuResults2 = m2.AsIndexable();
        //    }
        //    FloatMath.AreApproximatelyEqual(gpuResults1, top.AsIndexable()).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(gpuResults2, bottom.AsIndexable()).Should().BeTrue();

        //    IIndexableFloatMatrix simpleResults1, simpleResults2;
        //    using (var simpleA = _simple.CreateMatrix(a)) {
        //        var (top2, bottom2) = simpleA.SplitAtRow(POSITION);
        //        using var m1 = top2;
        //        using var m2 = bottom2;
        //        simpleResults1 = m1.AsIndexable();
        //        simpleResults2 = m2.AsIndexable();
        //    }
        //    FloatMath.AreApproximatelyEqual(simpleResults1, top.AsIndexable()).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults2, bottom.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSplitRows()
        //{
        //    const int POSITION = 2000;
        //    var rand = new Random();
        //    var a = _cpu.CreateMatrix(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
        //    var (left, right) = a.SplitAtColumn(POSITION);

        //    IIndexableFloatMatrix gpuResults1, gpuResults2;
        //    using (var gpuA = _cuda.CreateMatrix(a)) {
        //        var (left2, right2) = gpuA.SplitAtColumn(POSITION);
        //        using var m1 = left2;
        //        using var m2 = right2;
        //        gpuResults1 = m1.AsIndexable();
        //        gpuResults2 = m2.AsIndexable();
        //    }
        //    FloatMath.AreApproximatelyEqual(gpuResults1, left.AsIndexable()).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(gpuResults2, right.AsIndexable()).Should().BeTrue();

        //    IIndexableFloatMatrix simpleResults1, simpleResults2;
        //    using (var simpleA = _simple.CreateMatrix(a)) {
        //        var (left2, right2) = simpleA.SplitAtColumn(POSITION);
        //        using var m1 = left2;
        //        using var m2 = right2;
        //        simpleResults1 = m1.AsIndexable();
        //        simpleResults2 = m2.AsIndexable();
        //    }
        //    FloatMath.AreApproximatelyEqual(simpleResults1, left.AsIndexable()).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleResults2, right.AsIndexable()).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixL1Regularisation()
        //{
        //    var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
        //    const float OPERAND = 2f;

        //    var gpuResults = Apply(_cuda, a, a => a.L1Regularisation(OPERAND));
        //    var simpleResults = Apply(_simple, a, a => a.L1Regularisation(OPERAND));

        //    a.L1Regularisation(OPERAND);
        //    FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(a, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixColumnL2Norm()
        //{
        //    var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
        //    var cpuResults = a.ColumnL2Norm().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.ColumnL2Norm());
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.ColumnL2Norm());
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixRowL2Norm()
        //{
        //    var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
        //    var cpuResults = a.RowL2Norm().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.RowL2Norm());
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.RowL2Norm());
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixPointwiseDivideRows()
        //{
        //    var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
        //    var b = _cpu.CreateVector(6, i => i).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.PointwiseDivideRows(b));
        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.PointwiseDivideRows(b));

        //    a.PointwiseDivideRows(b);
        //    FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(a, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixPointwiseDivideColumns()
        //{
        //    var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
        //    var b = _cpu.CreateVector(3, i => i).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, b, (a, b) => a.PointwiseDivideColumns(b));
        //    var simpleResults = Apply(_simple, a, b, (a, b) => a.PointwiseDivideColumns(b));

        //    a.PointwiseDivideColumns(b);
        //    FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(a, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixDiagonal()
        //{
        //    var a = _cpu.CreateMatrix(6, 6, (x, y) => x * 2 + y).AsIndexable();
        //    var cpuResults = a.Diagonal().AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.Diagonal());
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.Diagonal());
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixPow()
        //{
        //    var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
        //    const float OPERAND = 2.5f;
        //    var cpuResults = a.Pow(OPERAND).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.Pow(OPERAND));
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.Pow(OPERAND));
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixGetRowSegment()
        //{
        //    var a = _cpu.CreateMatrix(12, 18, (x, y) => x * 2 + y).AsIndexable();
        //    var cpuResults = a.GetRowSegment(1, 2, 5).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.GetRowSegment(1, 2, 5));
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.GetRowSegment(1, 2, 5));
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixGetColumnSegment()
        //{
        //    var a = _cpu.CreateMatrix(9, 8, (x, y) => (x + 1) * (y + 1)).AsIndexable();
        //    var cpuResults = a.GetColumnSegment(1, 2, 5).AsIndexable();

        //    var gpuResults = Apply(_cuda, a, a => a.GetColumnSegment(1, 2, 5));
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    var simpleResults = Apply(_simple, a, a => a.GetColumnSegment(1, 2, 5));
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixConstrain()
        //{
        //    var distribution = new Normal(0, 5);
        //    var cpu = _cpu.CreateMatrix(100, 100, (x, y) => Convert.ToSingle(distribution.Sample())).AsIndexable();

        //    var gpuResults = Apply(_cuda, cpu, a => a.Constrain(-2f, 2f));
        //    var simpleResults = Apply(_simple, cpu, a => a.Constrain(-2f, 2f));

        //    cpu.Constrain(-2f, 2f);
        //    FloatMath.AreApproximatelyEqual(cpu, gpuResults).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(cpu, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void TestIdentity()
        //{
        //    var a = _cpu.CreateIdentityMatrix(1000).AsIndexable();

        //    IIndexableFloatMatrix a2;
        //    using (var gpuA = _cuda.CreateIdentityMatrix(1000))
        //        a2 = gpuA.AsIndexable();
        //    FloatMath.AreApproximatelyEqual(a, a2).Should().BeTrue();

        //    IIndexableFloatMatrix a3;
        //    using (var simpleA = _simple.CreateIdentityMatrix(1000))
        //        a3 = simpleA.AsIndexable();
        //    FloatMath.AreApproximatelyEqual(a, a3).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixSvd()
        //{
        //    var a = _cpu.CreateZeroMatrix(2, 2).AsIndexable();
        //    a[0, 0] = 4;
        //    a[0, 1] = 7;
        //    a[1, 0] = 2;
        //    a[1, 1] = 6;

        //    var (u, s, vt) = a.Svd();
        //    var cpuU = u.AsIndexable();
        //    var cpuVt = vt.AsIndexable();
        //    var cpuS = s.AsIndexable();

        //    IIndexableFloatMatrix gpuU, gpuVt;
        //    IIndexableFloatVector gpuS;
        //    using (var gpuA = _cuda.CreateMatrix(a)) {
        //        var (u2, s2, vt2) = gpuA.Svd();
        //        gpuU = u2.AsIndexable();
        //        gpuVt = vt2.AsIndexable();
        //        gpuS = s2.AsIndexable();
        //    }

        //    FloatMath.AreApproximatelyEqual(cpuU, gpuU).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(cpuVt, gpuVt).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(cpuS, gpuS).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixToVector()
        //{
        //    var matrix = _cpu.CreateMatrix(3, 4, (x, y) => (x + 1) * (y + 1)).AsIndexable();
        //    var vector = matrix.ReshapeAsVector();
        //    var matrix2 = vector.ReshapeAsMatrix(3, 4);
        //    FloatMath.AreApproximatelyEqual(matrix.AsIndexable(), matrix2.AsIndexable());

        //    using var gpuMatrix = _cuda.CreateMatrix(matrix.AsIndexable());
        //    using var gpuVector = gpuMatrix.ReshapeAsVector();
        //    using var gpuMatrix2 = gpuVector.ReshapeAsMatrix(3, 4);
        //    FloatMath.AreApproximatelyEqual(gpuMatrix.AsIndexable(), gpuMatrix2.AsIndexable()).Should().BeTrue();

        //    using var simpleMatrix = _simple.CreateMatrix(matrix.AsIndexable());
        //    using var simpleVector = simpleMatrix.ReshapeAsVector();
        //    using var simpleMatrix2 = simpleVector.ReshapeAsMatrix(3, 4);
        //    FloatMath.AreApproximatelyEqual(simpleMatrix.AsIndexable(), simpleMatrix2.AsIndexable()).Should().BeTrue();
        //}
    }
}
