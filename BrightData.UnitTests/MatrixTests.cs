using System;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class MatrixTests : CudaBase
    {
        [Fact]
        public void Simple()
        {
            using var matrix = _cpu.CreateMatrix(2, 2, false);
            matrix[0, 0] = 1;
            matrix[1, 0] = 2;
            matrix[0, 1] = 3;
            matrix[1, 1] = 4;
            var array = matrix.Segment.ToNewArray();
            array.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });

            matrix.GetColumnAsReadOnly(0).ToArray().Should().BeEquivalentTo(new[] { 1, 2 });
            matrix.GetColumnAsReadOnly(1).ToArray().Should().BeEquivalentTo(new[] { 3, 4 });
            matrix.GetRowAsReadOnly(0).ToArray().Should().BeEquivalentTo(new[] { 1, 3 });
            matrix.GetRowAsReadOnly(1).ToArray().Should().BeEquivalentTo(new[] { 2, 4 });
        }

        [Fact]
        public void SimpleTranspose()
        {
            using var matrix = _cpu.CreateMatrix(2, 2, false);
            matrix[0, 0] = 1;
            matrix[1, 0] = 2;
            matrix[0, 1] = 3;
            matrix[1, 1] = 4;

            using var transpose = matrix.Transpose();
            transpose.Segment.ToNewArray().Should().BeEquivalentTo(new[] { 1, 3, 2, 4 });
        }

        [Fact]
        public void SimpleTranspose2()
        {
            using var matrix = _cpu.CreateMatrix(2, 3, false);
            matrix[0, 0] = 1;
            matrix[1, 0] = 2;
            matrix[0, 1] = 3;
            matrix[1, 1] = 4;
            matrix[0, 2] = 5;
            matrix[1, 2] = 6;

            using var transpose = matrix.Transpose();
            transpose.Segment.ToNewArray().Should().BeEquivalentTo(new[] { 1, 3, 5, 2, 4, 6 });
        }

        [Fact]
        public void Multiply2X2()
        {
            using var matrix = _cpu.CreateMatrix(2, 2, false);
            matrix[0, 0] = 1;
            matrix[1, 0] = 2;
            matrix[0, 1] = 3;
            matrix[1, 1] = 4;
            using var matrix2 = matrix.Clone();

            using var multiplied = matrix.Multiply(matrix2);
            multiplied.Segment.ToNewArray().Should().BeEquivalentTo(new[] { 7, 10, 15, 22 });
        }

        [Fact]
        public void Multiply3X2()
        {
            var index = 1;
            using var matrix = _cpu.CreateMatrix(3, 2, (i, j) => index++);
            using var matrix2 = matrix.Transpose();

            using var multiplied = matrix.Multiply(matrix2);
            multiplied.Segment.ToNewArray().Should().BeEquivalentTo(new[] { 17, 22, 27, 22, 29, 36, 27, 36, 45 });
        }

        static IMatrix Apply(LinearAlgebraProvider lap, IMatrix a, IMatrix b, Func<IMatrix, IMatrix, IMatrix> func)
        {
            using var otherA = lap.CreateMatrix(a);
            using var otherB = lap.CreateMatrix(b);
            return func(otherA, otherB);
        }

        static IMatrix Apply(LinearAlgebraProvider lap, IMatrix a, IMatrix b, Action<IMatrix, IMatrix> func)
        {
            var otherA = lap.CreateMatrix(a);
            using var otherB = lap.CreateMatrix(b);
            func(otherA, otherB);
            return otherA;
        }

        static IMatrix Apply(LinearAlgebraProvider lap, IMatrix a, IVector b, Action<IMatrix, IVector> func)
        {
            var otherA = lap.CreateMatrix(a);
            using var otherB = lap.CreateVector(b);
            func(otherA, otherB);
            return otherA;
        }

        static IMatrix Apply(LinearAlgebraProvider lap, IMatrix a, Func<IMatrix, IMatrix> func)
        {
            using var otherA = lap.CreateMatrix(a);
            var otherB = func(otherA);
            return otherB;
        }

        static IMatrix Apply(LinearAlgebraProvider lap, IMatrix a, Action<IMatrix> func)
        {
            var otherA = lap.CreateMatrix(a);
            func(otherA);
            return otherA;
        }

        static T Apply<T>(LinearAlgebraProvider lap, IMatrix a, Func<IMatrix, T> func)
        {
            using var otherA = lap.CreateMatrix(a);
            var otherRow = func(otherA);
            return otherRow;
        }

        void CheckMatrixMultiplication(uint rowsA, uint columnsARowsB, uint columnsB)
        {
            var rand = new Random(1);
            var index = 1;
            using var a = _cpu.CreateMatrix(rowsA, columnsARowsB, (_, _) => index++/*rand.NextSingle()*/);
            using var b = _cpu.CreateMatrix(columnsARowsB, columnsB, (_, _) => index++/*rand.NextSingle()*/);
            using var cpu = a.Multiply(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.Multiply(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.Multiply(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TestMatrixCreationFromRows()
        {
            var values = new[] {
                Enumerable.Range(0, 10).Select(v => (float)v).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 2).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 3).ToArray(),
            };
            var cpuRowList = values.Select(v => _cpu.CreateVector(v)).ToArray();
            using var cpu = _cpu.CreateMatrixFromRows(cpuRowList);
            cpuRowList.DisposeAll();

            var gpuRowList = values.Select(v => _cuda.CreateVector(v)).ToArray();
            using var gpu = _cuda.CreateMatrixFromRows(gpuRowList);
            gpuRowList.DisposeAll();

            var mklRowList = values.Select(v => _mkl.CreateVector(v)).ToArray();
            using var mkl = _mkl.CreateMatrixFromRows(mklRowList);
            gpuRowList.DisposeAll();

            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TestMatrixCreationFromColumns()
        {
            var values = new[] {
                Enumerable.Range(0, 10).Select(v => (float)v).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 2).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 3).ToArray(),
            };
            var cpuRowList = values.Select(v => _cpu.CreateVector(v)).ToArray();
            var cpu = _cpu.CreateMatrixFromColumns(cpuRowList);
            cpuRowList.DisposeAll();

            var gpuRowList = values.Select(v => _cuda.CreateVector(v)).ToArray();
            using var gpu = _cuda.CreateMatrixFromColumns(gpuRowList);
            gpuRowList.DisposeAll();

            var mklRowList = values.Select(v => _mkl.CreateVector(v)).ToArray();
            using var mkl = _mkl.CreateMatrixFromColumns(mklRowList);
            mklRowList.DisposeAll();

            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixMultiplication()
        {
            CheckMatrixMultiplication(5, 2, 5);
        }

        [Fact]
        public void MatrixMultiplication2()
        {
            CheckMatrixMultiplication(500, 200, 500);
        }

        [Fact]
        public void MatrixMultiplication3()
        {
            CheckMatrixMultiplication(5, 5, 5);
        }

        [Fact]
        public void MatrixMultiplication4()
        {
            CheckMatrixMultiplication(5, 10, 5);
        }

        [Fact]
        public void MatrixMultiplication5()
        {
            CheckMatrixMultiplication(5, 10, 2);
        }

        [Fact]
        public void MatrixMultiplication6()
        {
            CheckMatrixMultiplication(50, 10, 2);
        }

        [Fact]
        public void MatrixMultiplication7()
        {
            CheckMatrixMultiplication(2, 10, 20);
        }

        [Fact]
        public void MatrixMultiplication8()
        {
            CheckMatrixMultiplication(20, 1, 19);
        }

        void Transpose(uint rows, uint columns)
        {
            var index = 1;
            using var a = _cpu.CreateMatrix(rows, columns, (j, k) => index++);
            using var cpu = a.Transpose();
            using var gpu = Apply(_cuda, a, a => a.Transpose());
            using var mkl = Apply(_mkl, a, a => a.Transpose());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixTranspose()
        {
            Transpose(2, 5);
        }

        [Fact]
        public void MatrixTranspose2()
        {
            Transpose(5, 2);
        }

        [Fact]
        public void MatrixTranspose3()
        {
            Transpose(500, 2);
        }

        [Fact]
        public void MatrixTranspose4()
        {
            Transpose(2, 500);
        }

        [Fact]
        public void MatrixTranspose5()
        {
            Transpose(20, 20);
        }

        [Fact]
        public void MatrixTranspose6()
        {
            Transpose(500, 500);
        }

        [Fact]
        public void MatrixTransposeAndMultiplication()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var b = _cpu.CreateMatrix(3, 5, (j, k) => j);
            using var cpu = a.TransposeAndMultiply(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.TransposeAndMultiply(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.TransposeAndMultiply(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixTransposeAndMultiplication2()
        {
            using var a = _cpu.CreateMatrix(2, 6, (j, k) => k);
            using var b = _cpu.CreateMatrix(2, 5, (j, k) => j);
            using var cpu = a.TransposeThisAndMultiply(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.TransposeThisAndMultiply(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.TransposeThisAndMultiply(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixAdd()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var b = _cpu.CreateMatrix(2, 5, (j, k) => j);
            using var cpu = a.Add(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.Add(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.Add(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSubtract()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var b = _cpu.CreateMatrix(2, 5, (j, k) => j);

            IMatrix gpu, gpu2;
            using var gpuA = _cuda.CreateMatrix(a);
            using var gpuB = _cuda.CreateMatrix(b);
            {
                var aStr = gpuA.ToString();
                var bStr = gpuB.ToString();
                gpu = gpuA.Subtract(gpuB);
                gpu2 = gpuB.Subtract(gpuA);

                aStr.Should().BeEquivalentTo(gpuA.ToString());
                bStr.Should().BeEquivalentTo(gpuB.ToString());
            }

            IMatrix mkl, mkl2;
            using var simpleA = _mkl.CreateMatrix(a);
            using var simpleB = _mkl.CreateMatrix(b);
            {
                var aStr = simpleA.ToString();
                var bStr = simpleB.ToString();
                mkl = simpleA.Subtract(simpleB);
                mkl2 = simpleB.Subtract(simpleA);

                aStr.Should().BeEquivalentTo(simpleA.ToString());
                bStr.Should().BeEquivalentTo(simpleB.ToString());
            }

            var cpu = a.Subtract(b);
            var cpu2 = b.Subtract(a);
            FloatMath.AreApproximatelyEqual(gpu, cpu).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(gpu2, cpu2).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(mkl, cpu).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(mkl2, cpu2).Should().BeTrue();
            gpu.Dispose();
            gpu2.Dispose();
            mkl.Dispose();
            mkl2.Dispose();
        }

        [Fact]
        public void MatrixPointwiseMultiply()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var b = _cpu.CreateMatrix(2, 5, (j, k) => j);
            using var cpu = a.PointwiseMultiply(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.PointwiseMultiply(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.PointwiseMultiply(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixPointwiseDivide()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k + 1);
            using var b = _cpu.CreateMatrix(2, 5, (j, k) => j + 1);
            using var cpu = a.PointwiseDivide(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.PointwiseDivide(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.PointwiseDivide(b));
            AssertSameWithMaxDifference(12, cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSqrt()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k + 1);
            a[0, 0] = -1e-8f;
            using var cpu = a.Sqrt();
            using var gpu = Apply(_cuda, a, a => a.Sqrt());
            using var mkl = Apply(_mkl, a, a => a.Sqrt());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixMultiplyScalar()
        {
            const float scalar = 2.5f;
            using var matrix = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var gpu = Apply(_cuda, matrix, a => a.Multiply(scalar));
            using var mkl = Apply(_mkl, matrix, a => a.Multiply(scalar));
            using var cpu = matrix.Multiply(scalar);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixColumn()
        {
            const int index = 7;
            using var a = _cpu.CreateMatrix(13, 17, (j, k) => (j + 1) * (k + 1));
            var cpu = a.GetColumnAsReadOnly(index).ReadOnlySegment.ToNewArray();
            var gpu = Apply(_cuda, a, a => a.GetColumnAsReadOnly(index).ReadOnlySegment.ToNewArray());
            var mkl = Apply(_mkl, a, a => a.GetColumnAsReadOnly(index).ReadOnlySegment.ToNewArray());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixColumnVector()
        {
            const int index = 7;
            using var a = _cpu.CreateMatrix(13, 17, (j, k) => (j + 1) * (k + 1));
            using var cpu = a.GetColumnVector(index);
            using var gpu = Apply(_cuda, a, a => a.GetColumnVector(index));
            using var mkl = Apply(_mkl, a, a => a.GetColumnVector(index));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixRow()
        {
            const int index = 11;
            using var a = _cpu.CreateMatrix(20, 50, (j, k) => k * j);
            var cpu = a.GetRowAsReadOnly(index).ReadOnlySegment.ToNewArray();
            var gpu = Apply(_cuda, a, a => a.GetRowAsReadOnly(index).ReadOnlySegment.ToNewArray());
            var mkl = Apply(_mkl, a, a => a.GetRowAsReadOnly(index).ReadOnlySegment.ToNewArray());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixRowVector()
        {
            const int index = 11;
            using var a = _cpu.CreateMatrix(20, 50, (j, k) => k * j);
            using var cpu = a.GetRowVector(index);
            using var gpu = Apply(_cuda, a, a => a.GetRowVector(index));
            using var mkl = Apply(_mkl, a, a => a.GetRowVector(index));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixRowSums()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var cpu = a.RowSums();
            var gpu = Apply(_cuda, a, a => a.RowSums());
            var mkl = Apply(_mkl, a, a => a.RowSums());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixColumnSums()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var cpu = a.ColumnSums();
            var gpu = Apply(_cuda, a, a => a.ColumnSums());
            var mkl = Apply(_mkl, a, a => a.ColumnSums());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixAddInPlace()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var b = _cpu.CreateMatrix(2, 5, (j, k) => j);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.AddInPlace(b, 1.5f, 2.5f));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.AddInPlace(b, 1.5f, 2.5f));
            a.AddInPlace(b, 1.5f, 2.5f);
            AssertSame(a, gpu, mkl);
        }

        [Fact]
        public void MatrixSubtractInPlace()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k);
            using var b = _cpu.CreateMatrix(2, 5, (j, k) => j);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.SubtractInPlace(b, 1.5f, 2.5f));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.SubtractInPlace(b, 1.5f, 2.5f));
            a.SubtractInPlace(b, 1.5f, 2.5f);
            AssertSame(a, gpu, mkl);
        }

        [Fact]
        public void MatrixAddToEachRow()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j);
            using var b = _cpu.CreateVector(5, (i) => i);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.AddToEachRow(b.Segment));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.AddToEachRow(b.Segment));

            a.AddToEachRow(b.Segment);
            AssertSame(a, gpu, mkl);
        }

        [Fact]
        public void MatrixAddToEachColumn()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j);
            using var b = _cpu.CreateVector(2, (i) => i + 5);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.AddToEachColumn(b.Segment));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.AddToEachColumn(b.Segment));

            a.AddToEachColumn(b.Segment);
            AssertSame(a, gpu, mkl);
        }

        [Fact]
        public void MatrixMultiplyEachRowWith()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j);
            using var b = _cpu.CreateVector(5, (i) => i);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.MultiplyEachRowWith(b.Segment));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.MultiplyEachRowWith(b.Segment));

            a.MultiplyEachRowWith(b.Segment);
            AssertSame(a, gpu, mkl);
        }

        [Fact]
        public void MatrixMultiplyEachColumnWith()
        {
            using var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j);
            using var b = _cpu.CreateVector(2, (i) => i + 5);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.MultiplyEachColumnWith(b.Segment));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.MultiplyEachColumnWith(b.Segment));

            a.MultiplyEachColumnWith(b.Segment);
            AssertSame(a, gpu, mkl);
        }

        [Fact]
        public void MatrixSigmoidActivation()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.Sigmoid();
            using var gpu = Apply(_cuda, a, (a) => a.Sigmoid());
            using var mkl = Apply(_mkl, a, (a) => a.Sigmoid());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSigmoidDerivative()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.SigmoidDerivative();
            using var gpu = Apply(_cuda, a, (a) => a.SigmoidDerivative());
            using var mkl = Apply(_mkl, a, (a) => a.SigmoidDerivative());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixTanhActivation()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.Tanh();
            using var gpu = Apply(_cuda, a, a => a.Tanh());
            using var mkl = Apply(_mkl, a, (a) => a.Tanh());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixTanhDerivative()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.TanhDerivative();
            using var gpu = Apply(_cuda, a, a => a.TanhDerivative());
            using var mkl = Apply(_cuda, a, a => a.TanhDerivative());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixReluActivation()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.Relu();
            using var gpu = Apply(_cuda, a, a => a.Relu());
            using var mkl = Apply(_cuda, a, a => a.Relu());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixReluDerivative()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.ReluDerivative();
            using var gpu = Apply(_cuda, a, a => a.ReluDerivative());
            using var mkl = Apply(_cuda, a, a => a.ReluDerivative());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixLeakyReluActivation()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.LeakyRelu();
            using var gpu = Apply(_cuda, a, a => a.LeakyRelu());
            using var mkl = Apply(_cuda, a, a => a.LeakyRelu());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixLeakyReluDerivative()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.LeakyReluDerivative();
            using var gpu = Apply(_cuda, a, a => a.LeakyReluDerivative());
            using var mkl = Apply(_cuda, a, a => a.LeakyReluDerivative());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSoftmaxActivation()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var cpu = a.Softmax();
            using var gpu = Apply(_cuda, a, a => a.Softmax());
            using var mkl = Apply(_cuda, a, a => a.Softmax());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSoftmaxPerRow()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            var cpu = a.SoftmaxPerRow().Select(x => _cpu.CreateVector(x)).ToArray();
            var gpu = Apply(_cuda, a, a => a.SoftmaxPerRow().Select(x => _cuda.CreateVector(x)).ToArray());
            var mkl = Apply(_mkl, a, a => a.SoftmaxPerRow().Select(x => _mkl.CreateVector(x)).ToArray());
            AssertSameAndThenDispose(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixRowVector2()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            var cpu = 3.AsRange().Select(a.GetRowVector).ToArray();
            var gpu = Apply(_cuda, a, a => 3.AsRange().Select(a.GetRowVector).ToArray());
            var mkl = Apply(_mkl, a, a => 3.AsRange().Select(a.GetRowVector).ToArray());
            AssertSameAndThenDispose(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixRowVectorsTransposed()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            using var aT = a.Transpose();
            var cpu = 3.AsRange().Select(aT.GetColumnVector).ToArray();
            var gpu = Apply(_cuda, aT, aT => 3.AsRange().Select(aT.GetColumnVector).ToArray());
            var mkl = Apply(_mkl, aT, aT => 3.AsRange().Select(aT.GetColumnVector).ToArray());
            AssertSameAndThenDispose(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSoftmaxDerivative()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            var cpu = _cpu.SoftmaxDerivative(a.Segment);
            var gpu = Apply(_cuda, a, a => _cuda.SoftmaxDerivative(a.Segment));
            var mkl = Apply(_mkl, a, a => _mkl.SoftmaxDerivative(a.Segment));
            AssertSameAndThenDispose(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSoftmaxPerRowDerivative()
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample()));
            var cpu = a.SoftmaxPerRow();
            var gpu = Apply(_cuda, a, a => a.SoftmaxPerRow());
            var mkl = Apply(_mkl, a, a => a.SoftmaxPerRow());

            var cpu2 = a.SoftmaxDerivativePerRow(cpu);
            var gpu2 = Apply(_cuda, a, a2 => a2.SoftmaxDerivativePerRow(gpu));
            var mkl2 = Apply(_mkl, a, a2 => a2.SoftmaxDerivativePerRow(mkl));

            AssertSameAndThenDispose(8, cpu, gpu, mkl);
            AssertSameAndThenDispose(8, cpu2, gpu2, mkl2);
        }

        [Fact]
        public void MatrixNewMatrixFromRows()
        {
            var rows = new uint[] { 7, 8, 9 };
            using var a = _cpu.CreateMatrix(13, 17, (j, k) => (k + 1) * (j + 1));
            using var cpu = a.GetNewMatrixFromRows(rows);
            using var gpu = Apply(_cuda, a, a => a.GetNewMatrixFromRows(rows));
            using var mkl = Apply(_mkl, a, a => a.GetNewMatrixFromRows(rows));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixNewMatrixFromColumns()
        {
            var cols = new uint[] { 1, 2, 9 };
            using var a = _cpu.CreateMatrix(12, 13, (j, k) => (k + 1) * (j + 1));
            using var cpu = a.GetNewMatrixFromColumns(cols);
            using var gpu = Apply(_cuda, a, a => a.GetNewMatrixFromColumns(cols));
            using var mkl = Apply(_mkl, a, a => a.GetNewMatrixFromColumns(cols));
            AssertSame(cpu, gpu, mkl);
        }

        //[Fact]
        //public void MatrixClearRows()
        //{
        //    var rows = new uint[] { 1, 2, 9 };
        //    using var a = _cpu.CreateMatrix(13, 12, (j, k) => k + 1);
        //    using var gpu = Apply(_cuda, a, a => a.ClearRows(rows));
        //    using var mkl = Apply(_mkl, a, a => a.ClearRows(rows));
        //    a.ClearRows(rows);
        //    AssertSame(a, gpu, mkl);
        //}

        //[Fact]
        //public void MatrixClearColumns()
        //{
        //    var cols = new uint[] { 1, 2, 7 };
        //    using var a = _cpu.CreateMatrix(18, 13, (j, k) => k + 1);
        //    using var gpu = Apply(_cuda, a, a => a.ClearColumns(cols));
        //    using var mkl = Apply(_mkl, a, a => a.ClearColumns(cols));
        //    a.ClearColumns(cols);
        //    AssertSame(a, gpu, mkl);
        //}

        [Fact]
        public void MatrixClear()
        {
            using var a = _cpu.CreateMatrix(15, 23, (j, k) => k + 1);
            using var gpu = Apply(_cuda, a, a => a.Clear());
            using var mkl = Apply(_mkl, a, a => a.Clear());
            a.Clear();
            AssertSame(a, gpu, mkl);
        }

        [Fact]
        public void MatrixClone()
        {
            using var a = _cpu.CreateMatrix(12, 7, (j, k) => k + 1);
            using var cpu = a.Clone();
            using var gpu = Apply(_cuda, a, a => a.Clone());
            using var mkl = Apply(_mkl, a, a => a.Clone());
            AssertSame(cpu, gpu, mkl);
        }

        //[Fact]
        //public void MatrixReadWrite()
        //{
        //    var a = _cpu.CreateMatrix(7, 20, (x, y) => x * 10 + y);

        //    // test Numerics -> Numerics serialisation
        //    var serialised = a.Data;
        //    var b = _cpu.CreateMatrix(serialised);
        //    FloatMath.AreApproximatelyEqual(a, b).Should().BeTrue();

        //    // test Numerics -> Cuda serialisation
        //    using var c = _cuda.CreateMatrix(serialised);
        //    FloatMath.AreApproximatelyEqual(a, c).Should().BeTrue();

        //    // test Cuda -> Cuda serialisation
        //    var serialised2 = c.Data;
        //    using (var d = _cuda.CreateMatrix(serialised2))
        //        FloatMath.AreApproximatelyEqual(a, d).Should().BeTrue();

        //    // test Cuda -> Numerics serialisation
        //    var e = _cpu.CreateMatrix(c.Data);
        //    FloatMath.AreApproximatelyEqual(a, e).Should().BeTrue();
        //}

        [Fact]
        public void MatrixConcatColumns()
        {
            var rand = new Random();
            using var a = _cpu.CreateMatrix(4000, 300, (x, y) => Convert.ToSingle(rand.NextDouble()));
            using var b = _cpu.CreateMatrix(200, 300, (x, y) => Convert.ToSingle(rand.NextDouble()));
            using var cpu = a.ConcatBelow(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.ConcatBelow(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.ConcatBelow(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixConcatRows()
        {
            var rand = new Random();
            using var a = _cpu.CreateMatrix(300, 4000, (x, y) => Convert.ToSingle(rand.NextDouble()));
            using var b = _cpu.CreateMatrix(300, 200, (x, y) => Convert.ToSingle(rand.NextDouble()));
            using var cpu = a.ConcatRight(b);
            using var gpu = Apply(_cuda, a, b, (a, b) => a.ConcatRight(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.ConcatRight(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixSplitColumns()
        {
            const int position = 2000;
            var rand = new Random();
            using var a = _cpu.CreateMatrix(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var (top, bottom) = a.SplitAtRow(position);

            using (var gpuA = _cuda.CreateMatrix(a)) {
                var (top2, bottom2) = gpuA.SplitAtRow(position);
                using var m1 = top2;
                using var m2 = bottom2;
                FloatMath.AreApproximatelyEqual(m1, top).Should().BeTrue();
                FloatMath.AreApproximatelyEqual(m2, bottom).Should().BeTrue();
            }

            using (var simpleA = _mkl.CreateMatrix(a)) {
                var (top2, bottom2) = simpleA.SplitAtRow(position);
                using var m1 = top2;
                using var m2 = bottom2;
                FloatMath.AreApproximatelyEqual(m1, top).Should().BeTrue();
                FloatMath.AreApproximatelyEqual(m2, bottom).Should().BeTrue();
            }
            top.Dispose();
            bottom.Dispose();
        }

        [Fact]
        public void MatrixSplitRows()
        {
            const int position = 2000;
            var rand = new Random();
            using var a = _cpu.CreateMatrix(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var (left, right) = a.SplitAtColumn(position);

            using var gpuA = _cuda.CreateMatrix(a);
            {
                var (left2, right2) = gpuA.SplitAtColumn(position);
                using var m1 = left2;
                using var m2 = right2;
                FloatMath.AreApproximatelyEqual(m1, left).Should().BeTrue();
                FloatMath.AreApproximatelyEqual(m2, right).Should().BeTrue();
            }

            using (var simpleA = _mkl.CreateMatrix(a)) {
                var (left2, right2) = simpleA.SplitAtColumn(position);
                using var m1 = left2;
                using var m2 = right2;
                FloatMath.AreApproximatelyEqual(m1, left).Should().BeTrue();
                FloatMath.AreApproximatelyEqual(m2, right).Should().BeTrue();
            }
            left.Dispose();
            right.Dispose();
        }

        [Fact]
        public void MatrixL1Regularisation()
        {
            using var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y);
            const float operand = 2f;
            using var gpu = Apply(_cuda, a, a => a.L1RegularisationInPlace(operand));
            using var mkl = Apply(_mkl, a, a => a.L1RegularisationInPlace(operand));
            a.L1RegularisationInPlace(operand);
            AssertSame(a, gpu, mkl);
        }

        //[Fact]
        //public void MatrixColumnL2Norm()
        //{
        //    var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y);
        //    using var cpu = a.ColumnL2Norm();
        //    using var gpu = Apply(_cuda, a, a => a.ColumnL2Norm());
        //    using var mkl = Apply(_mkl, a, a => a.ColumnL2Norm());
        //    AssertSame(cpu, gpu, mkl);
        //}

        //[Fact]
        //public void MatrixRowL2Norm()
        //{
        //    using var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y);
        //    using var cpu = a.RowL2Norm();
        //    using var gpu = Apply(_cuda, a, a => a.RowL2Norm());
        //    using var mkl = Apply(_mkl, a, a => a.RowL2Norm());
        //    AssertSame(cpu, gpu, mkl);
        //}

        //[Fact]
        //public void MatrixPointwiseDivideRows()
        //{
        //    using var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y);
        //    using var b = _cpu.CreateVector(6, i => i);
        //    using var gpu = Apply(_cuda, a, b, (a, b) => a.PointwiseDivideRows(b));
        //    using var mkl = Apply(_mkl, a, b, (a, b) => a.PointwiseDivideRows(b));
        //    a.PointwiseDivideRows(b);
        //    AssertSame(a, gpu, mkl);
        //}

        //[Fact]
        //public void MatrixPointwiseDivideColumns()
        //{
        //    using var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y);
        //    using var b = _cpu.CreateVector(3, i => i);
        //    using var gpu = Apply(_cuda, a, b, (a, b) => a.PointwiseDivideColumns(b));
        //    using var mkl = Apply(_mkl, a, b, (a, b) => a.PointwiseDivideColumns(b));
        //    a.PointwiseDivideColumns(b);
        //    AssertSame(a, gpu, mkl);
        //}

        //[Fact]
        //public void MatrixDiagonal()
        //{
        //    using var a = _cpu.CreateMatrix(6, 6, (x, y) => x * 2 + y);
        //    using var cpu = a.Diagonal();
        //    using var gpu = Apply(_cuda, a, a => a.Diagonal());
        //    using var mkl = Apply(_mkl, a, a => a.Diagonal());
        //    AssertSame(cpu, gpu, mkl);
        //}

        [Fact]
        public void MatrixPow()
        {
            using var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y);
            const float operand = 2.5f;
            using var cpu = a.Pow(operand);
            using var gpu = Apply(_cuda, a, a => a.Pow(operand));
            using var mkl = Apply(_mkl, a, a => a.Pow(operand));
            AssertSameWithMaxDifference(12, cpu, gpu, mkl);
        }

        //[Fact]
        //public void MatrixGetRowSegment()
        //{
        //    using var a = _cpu.CreateMatrix(12, 18, (x, y) => x * 2 + y);
        //    using var cpu = a.GetRowSegment(1, 2, 5);
        //    using var gpu = Apply(_cuda, a, a => a.GetRowSegment(1, 2, 5));
        //    using var mkl = Apply(_mkl, a, a => a.GetRowSegment(1, 2, 5));
        //    AssertSame(cpu, gpu, mkl);
        //}

        //[Fact]
        //public void MatrixGetColumnSegment()
        //{
        //    using var a = _cpu.CreateMatrix(9, 8, (x, y) => (x + 1) * (y + 1));
        //    using var cpu = a.GetColumnSegment(1, 2, 5);
        //    using var gpu = Apply(_cuda, a, a => a.GetColumnSegment(1, 2, 5));
        //    using var mkl = Apply(_mkl, a, a => a.GetColumnSegment(1, 2, 5));
        //    AssertSame(cpu, gpu, mkl);
        //}

        [Fact]
        public void MatrixConstrain()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);
            using var cpu = _cpu.CreateMatrix(100, 100, (x, y) => Convert.ToSingle(distribution.Sample()));
            using var gpu = Apply(_cuda, cpu, a => a.ConstrainInPlace(-2f, 2f));
            using var mkl = Apply(_mkl, cpu, a => a.ConstrainInPlace(-2f, 2f));
            cpu.ConstrainInPlace(-2f, 2f);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TestIdentity()
        {
            using var a = _cpu.CreateIdentityMatrix(1000);
            using var gpuA = _cuda.CreateIdentityMatrix(1000);
            using var a2 = gpuA;
            using var simpleA = _mkl.CreateIdentityMatrix(1000);
            using var a3 = simpleA;
            AssertSame(a, a2, a3);
        }

        //[Fact]
        //public void MatrixSvd()
        //{
        //    using var a = _mkl.CreateMatrix(2, 2);
        //    a[0, 0] = 4;
        //    a[0, 1] = 7;
        //    a[1, 0] = 2;
        //    a[1, 1] = 6;

        //    var (u, s, vt) = a.Svd();
        //    using var cpuU = u;
        //    using var cpuVt = vt;
        //    using var cpuS = s;

        //    using var gpuA = _cuda.CreateMatrix(a);
        //    var (u2, s2, vt2) = gpuA.Svd();
        //    using var gpuU = u2;
        //    using var gpuVt = vt2;
        //    using var gpuS = s2;

        //    FloatMath.AreApproximatelyEqual(cpuU, gpuU).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(cpuVt, gpuVt).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(cpuS, gpuS).Should().BeTrue();
        //}

        [Fact]
        public void MatrixToVector()
        {
            using var matrix = _cpu.CreateMatrix(3, 4, (x, y) => (x + 1) * (y + 1));
            using var vector = matrix.Reshape();
            using var matrix2 = vector.Reshape(3, 4);
            FloatMath.AreApproximatelyEqual(matrix, matrix2);

            using var gpuMatrix = _cuda.CreateMatrix(matrix);
            using var gpuVector = gpuMatrix.Reshape();
            using var gpuMatrix2 = gpuVector.Reshape(3, 4);
            FloatMath.AreApproximatelyEqual(gpuMatrix, gpuMatrix2).Should().BeTrue();

            using var mklMatrix = _mkl.CreateMatrix(matrix);
            using var mklVector = mklMatrix.Reshape();
            using var mklMatrix2 = mklVector.Reshape(3, 4);
            FloatMath.AreApproximatelyEqual(mklMatrix, mklMatrix2).Should().BeTrue();

            AssertSame(matrix2, gpuMatrix2, mklMatrix2);
        }
    }
}
