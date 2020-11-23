using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData.Helper;
using FluentAssertions;
using MathNet.Numerics.Distributions;
using Xunit;

namespace BrightData.UnitTests
{
    public class MatrixTests : CudaBase
    {
        void _MatrixMultiplication(uint rowsA, uint columnsArowsB, uint columnsB)
        {
            var rand = new Random(1);
            var a = _cpu.CreateMatrix(rowsA, columnsArowsB, (j, k) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var b = _cpu.CreateMatrix(columnsArowsB, columnsB, (j, k) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var cpuResults = a.Multiply(b);
            IIndexableFloatMatrix gpuResults;

            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var gpuC = gpuA.Multiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void TestMatrixCreationFromRows()
        {
            var values = new[] {
                Enumerable.Range(0, 10).Select(v => (float)v).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 2).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 3).ToArray(),
            };
            var cpuRowList = values.Select(v => _cpu.CreateVector(v)).ToList();
            var cpuMatrix = _cpu.CreateMatrixFromRows(cpuRowList);

            var gpuRowList = values.Select(v => _cuda.CreateVector(v)).ToList();
            using (var gpuMatrix = _cuda.CreateMatrixFromRows(gpuRowList)) {
                FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable()).Should().BeTrue();
            }
            gpuRowList.ForEach(v => v.Dispose());
        }

        [Fact]
        public void TestMatrixCreationFromColumns()
        {
            var values = new[] {
                Enumerable.Range(0, 10).Select(v => (float)v).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 2).ToArray(),
                Enumerable.Range(0, 10).Select(v => (float)v * 3).ToArray(),
            };
            var cpuRowList = values.Select(v => _cpu.CreateVector(v)).ToList();
            var cpuMatrix = _cpu.CreateMatrixFromColumns(cpuRowList);

            var gpuRowList = values.Select(v => _cuda.CreateVector(v)).ToList();
            using (var gpuMatrix = _cuda.CreateMatrixFromColumns(gpuRowList)) {
                FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable()).Should().BeTrue();
            }
            gpuRowList.ForEach(v => v.Dispose());
        }

        [Fact]
        public void MatrixMultiplication()
        {
            _MatrixMultiplication(5, 2, 5);
        }

        [Fact]
        public void MatrixMultiplication2()
        {
            _MatrixMultiplication(500, 200, 500);
        }

        [Fact]
        public void MatrixMultiplication3()
        {
            _MatrixMultiplication(5, 5, 5);
        }

        [Fact]
        public void MatrixMultiplication4()
        {
            _MatrixMultiplication(5, 10, 5);
        }

        [Fact]
        public void MatrixMultiplication5()
        {
            _MatrixMultiplication(5, 10, 2);
        }

        [Fact]
        public void MatrixMultiplication6()
        {
            _MatrixMultiplication(50, 10, 2);
        }

        [Fact]
        public void MatrixMultiplication7()
        {
            _MatrixMultiplication(2, 10, 20);
        }

        [Fact]
        public void MatrixMultiplication8()
        {
            _MatrixMultiplication(20, 1, 19);
        }

        void _Transpose(uint rows, uint columns)
        {
            var a = _cpu.CreateMatrix(rows, columns, (j, k) => k).AsIndexable();
            var aT = a.Transpose();
            IIndexableFloatMatrix gpuResults;

            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuAT = gpuA.Transpose())
                gpuResults = gpuAT.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, aT.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixTranspose()
        {
            _Transpose(2, 5);
        }

        [Fact]
        public void MatrixTranspose2()
        {
            _Transpose(5, 2);
        }

        [Fact]
        public void MatrixTranspose3()
        {
            _Transpose(500, 2);
        }

        [Fact]
        public void MatrixTranspose4()
        {
            _Transpose(2, 500);
        }

        [Fact]
        public void MatrixTranspose5()
        {
            _Transpose(20, 20);
        }

        [Fact]
        public void MatrixTranspose6()
        {
            _Transpose(500, 500);
        }

        [Fact]
        public void MatrixTransposeAndMultiplication()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var b = _cpu.CreateMatrix(3, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.TransposeAndMultiply(b);
            IIndexableFloatMatrix gpuResults;

            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var gpuC = gpuA.TransposeAndMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixTransposeAndMultiplication2()
        {
            var a = _cpu.CreateMatrix(2, 6, (j, k) => k).AsIndexable();
            var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.TransposeThisAndMultiply(b);
            IIndexableFloatMatrix gpuResults;

            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var gpuC = gpuA.TransposeThisAndMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixAdd()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.Add(b);

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var gpuC = gpuA.Add(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixSubtract()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();

            IIndexableFloatMatrix gpuResults, gpuResults2;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b)) {
                var aStr = gpuA.ToString();
                var bStr = gpuB.ToString();
                using (var gpuC = gpuA.Subtract(gpuB))
                using (var gpuD = gpuB.Subtract(gpuA)) {
                    gpuResults = gpuC.AsIndexable();
                    gpuResults2 = gpuD.AsIndexable();
                }

                aStr.Should().BeEquivalentTo(gpuA.ToString());
                bStr.Should().BeEquivalentTo(gpuB.ToString());
            }

            var cpuResults = a.Subtract(b);
            var cpuResults2 = b.Subtract(a);
            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(gpuResults2, cpuResults2.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixPointwiseMultiply()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.PointwiseMultiply(b);

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var gpuC = gpuA.PointwiseMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixPointwiseDivide()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k + 1).AsIndexable();
            var b = _cpu.CreateMatrix(2, 5, (j, k) => j + 1).AsIndexable();
            var cpuResults = a.PointwiseDivide(b);

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var gpuC = gpuA.PointwiseDivide(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixSqrt()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k + 1).AsIndexable();
            const float adjustment = 1e-8f;
            a[0, 0] = -adjustment;
            var cpuResults = a.Sqrt(adjustment);

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuC = gpuA.Sqrt(1e-8f))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixMultiplyScalar()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            const float scalar = 2.5f;

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                gpuA.Multiply(scalar);
                gpuResults = gpuA.AsIndexable();
            }

            a.Multiply(scalar);
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixColumn()
        {
            const int INDEX = 7;
            var a = _cpu.CreateMatrix(13, 17, (j, k) => (j + 1) * (k + 1)).AsIndexable();
            var row = a.Column(INDEX).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuCol = gpuA.Column(INDEX))
                gpuResults = gpuCol.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, row).Should().BeTrue();
        }

        [Fact]
        public void MatrixRow()
        {
            const int INDEX = 11;
            var a = _cpu.CreateMatrix(20, 50, (j, k) => k * j).AsIndexable();
            var row = a.Row(INDEX).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuRow = gpuA.Row(INDEX))
                gpuResults = gpuRow.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, row).Should().BeTrue();
        }

        [Fact]
        public void MatrixRowSums()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var rowSums = a.RowSums().AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuRowSums = gpuA.RowSums())
                gpuResults = gpuRowSums.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, rowSums).Should().BeTrue();
        }

        [Fact]
        public void MatrixColumnSums()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var colSums = a.ColumnSums().AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuColSums = gpuA.ColumnSums())
                gpuResults = gpuColSums.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, colSums).Should().BeTrue();
        }

        [Fact]
        public void MatrixAddInPlace()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b)) {
                gpuA.AddInPlace(gpuB, 1.5f, 2.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddInPlace(b, 1.5f, 2.5f);
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixSubtractInPlace()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k).AsIndexable();
            var b = _cpu.CreateMatrix(2, 5, (j, k) => j).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b)) {
                gpuA.SubtractInPlace(gpuB, 1.5f, 2.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.SubtractInPlace(b, 1.5f, 2.5f);
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixAddToEachRow()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j).AsIndexable();
            var b = _cpu.CreateVector(5, (i) => i).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateVector(b)) {
                gpuA.AddToEachRow(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddToEachRow(b);
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixAddToEachColumn()
        {
            var a = _cpu.CreateMatrix(2, 5, (j, k) => k * j).AsIndexable();
            var b = _cpu.CreateVector(2, (i) => i + 5).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateVector(b)) {
                gpuA.AddToEachColumn(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddToEachColumn(b);
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixSigmoidActivation()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.SigmoidActivation().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var sigmoid = gpuA.SigmoidActivation())
                gpuResults = sigmoid.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixSigmoidDerivative()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.SigmoidDerivative().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var sigmoid = gpuA.SigmoidDerivative())
                gpuResults = sigmoid.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixTanhActivation()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.TanhActivation().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var tanh = gpuA.TanhActivation())
                gpuResults = tanh.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixTanhDerivative()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.TanhDerivative().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var tanh = gpuA.TanhDerivative())
                gpuResults = tanh.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixRELUActivation()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.ReluActivation().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var relu = gpuA.ReluActivation())
                gpuResults = relu.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixRELUDerivative()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.ReluDerivative().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var relu = gpuA.ReluDerivative())
                gpuResults = relu.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixLeakyRELUActivation()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.LeakyReluActivation().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var relu = gpuA.LeakyReluActivation())
                gpuResults = relu.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixLeakyRELUDerivative()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.LeakyReluDerivative().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var relu = gpuA.LeakyReluDerivative())
                gpuResults = relu.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixSoftmaxActivation()
        {
            var normalDistribution = new Normal(0, 1);
            var a = _cpu.CreateMatrix(3, 7, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.SoftmaxActivation().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var softmax = gpuA.SoftmaxActivation())
                gpuResults = softmax.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixNewMatrixFromRows()
        {
            var rows = new uint[] { 7, 8, 9 };
            var a = _cpu.CreateMatrix(13, 17, (j, k) => (k + 1) * (j + 1)).AsIndexable();
            var results = a.GetNewMatrixFromRows(rows).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var m = gpuA.GetNewMatrixFromRows(rows))
                gpuResults = m.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, results).Should().BeTrue();
        }

        [Fact]
        public void MatrixNewMatrixFromColumns()
        {
            var cols = new uint[] { 1, 2, 9 };
            var a = _cpu.CreateMatrix(12, 13, (j, k) => (k + 1) * (j + 1)).AsIndexable();
            var results = a.GetNewMatrixFromColumns(cols).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var m = gpuA.GetNewMatrixFromColumns(cols))
                gpuResults = m.AsIndexable();

            FloatMath.AreApproximatelyEqual(gpuResults, results).Should().BeTrue();
        }

        [Fact]
        public void MatrixClearRows()
        {
            var rows = new uint[] { 1, 2, 9 };
            var a = _cpu.CreateMatrix(13, 12, (j, k) => k + 1).AsIndexable();
            a.ClearRows(rows);

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                a.ClearRows(rows);
                gpuResults = a.AsIndexable();
            }

            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixClearColumns()
        {
            var cols = new uint[] { 1, 2, 7 };
            var a = _cpu.CreateMatrix(18, 13, (j, k) => k + 1).AsIndexable();
            a.ClearColumns(cols);

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                gpuA.ClearColumns(cols);
                gpuResults = gpuA.AsIndexable();
            }

            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixClear()
        {
            var a = _cpu.CreateMatrix(15, 23, (j, k) => k + 1).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                gpuA.Clear();
                gpuResults = gpuA.AsIndexable();
            }

            a.Clear();
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }

        [Fact]
        public void MatrixClone()
        {
            var a = _cpu.CreateMatrix(12, 7, (j, k) => k + 1).AsIndexable();
            var b = a.Clone().AsIndexable();
            FloatMath.AreApproximatelyEqual(a, b);

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var clone = gpuA.Clone()) {
                gpuResults = clone.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void MatrixReadWrite()
        {
            var a = _cpu.CreateMatrix(7, 20, (x, y) => x * 10 + y).AsIndexable();

            // test Numerics -> Numerics serialisation
            var serialised = a.Data;
            var b = _cpu.CreateMatrix(serialised);
            FloatMath.AreApproximatelyEqual(a.AsIndexable(), b.AsIndexable()).Should().BeTrue();

            // test Numerics -> Cuda serialisation
            using var c = _cuda.CreateMatrix(serialised);
            FloatMath.AreApproximatelyEqual(a.AsIndexable(), c.AsIndexable()).Should().BeTrue();

            // test Cuda -> Cuda serialisation
            var serialised2 = c.Data;
            using (var d = _cuda.CreateMatrix(serialised2))
                FloatMath.AreApproximatelyEqual(a.AsIndexable(), d.AsIndexable()).Should().BeTrue();

            // test Cuda -> Numerics serialisation
            var e = _cpu.CreateMatrix(c.Data);
            FloatMath.AreApproximatelyEqual(a.AsIndexable(), e.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixConcatColumns()
        {
            var rand = new Random();
            var a = _cpu.CreateMatrix(4000, 300, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var b = _cpu.CreateMatrix(200, 300, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var c = a.ConcatColumns(b).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var concat = gpuA.ConcatColumns(gpuB)) {
                gpuResults = concat.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixConcatRows()
        {
            var rand = new Random();
            var a = _cpu.CreateMatrix(300, 4000, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var b = _cpu.CreateMatrix(300, 200, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var c = a.ConcatRows(b).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateMatrix(b))
            using (var concat = gpuA.ConcatRows(gpuB)) {
                gpuResults = concat.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixSplitColumns()
        {
            const int POSITION = 2000;
            var rand = new Random();
            var a = _cpu.CreateMatrix(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var r = a.SplitAtRow(POSITION);

            IIndexableFloatMatrix gpuResults1, gpuResults2;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                var r2 = gpuA.SplitAtRow(POSITION);
                using (var m1 = r2.Top)
                using (var m2 = r2.Bottom) {
                    gpuResults1 = m1.AsIndexable();
                    gpuResults2 = m2.AsIndexable();
                }
            }
            FloatMath.AreApproximatelyEqual(gpuResults1, r.Top.AsIndexable()).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(gpuResults2, r.Bottom.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixSplitRows()
        {
            const int POSITION = 2000;
            var rand = new Random();
            var a = _cpu.CreateMatrix(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var r = a.SplitAtColumn(POSITION);

            IIndexableFloatMatrix gpuResults1, gpuResults2;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                var r2 = gpuA.SplitAtColumn(POSITION);
                using (var m1 = r2.Left)
                using (var m2 = r2.Right) {
                    gpuResults1 = m1.AsIndexable();
                    gpuResults2 = m2.AsIndexable();
                }
            }
            FloatMath.AreApproximatelyEqual(gpuResults1, r.Left.AsIndexable()).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(gpuResults2, r.Right.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void MatrixL1Regularisation()
        {
            var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
            const float OPERAND = 2f;

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                gpuA.L1Regularisation(OPERAND);
                gpuResults = gpuA.AsIndexable();
            }
            a.L1Regularisation(OPERAND);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixColumnL2Norm()
        {
            var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
            var r = a.ColumnL2Norm().AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var norm = gpuA.ColumnL2Norm()) {
                gpuResults = norm.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(r, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixRowL2Norm()
        {
            var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
            var r = a.RowL2Norm().AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var norm = gpuA.RowL2Norm()) {
                gpuResults = norm.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(r, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixPointwiseDivideRows()
        {
            var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
            var b = _cpu.CreateVector(6, i => i).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateVector(b)) {
                gpuA.PointwiseDivideRows(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.PointwiseDivideRows(b);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixPointwiseDivideColumns()
        {
            var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
            var b = _cpu.CreateVector(3, i => i).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateVector(b)) {
                gpuA.PointwiseDivideColumns(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.PointwiseDivideColumns(b);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixDiagonal()
        {
            var a = _cpu.CreateMatrix(6, 6, (x, y) => x * 2 + y).AsIndexable();
            var d = a.Diagonal().AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var diagonal = gpuA.Diagonal()) {
                gpuResults = diagonal.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(d, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixPow()
        {
            var a = _cpu.CreateMatrix(6, 3, (x, y) => x * 2 + y).AsIndexable();
            const float OPERAND = 2.5f;
            var r = a.Pow(OPERAND).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var pow = gpuA.Pow(OPERAND)) {
                gpuResults = pow.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(r, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixGetRowSegment()
        {
            var a = _cpu.CreateMatrix(12, 18, (x, y) => x * 2 + y).AsIndexable();
            var r = a.GetRowSegment(1, 2, 5).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                gpuResults = gpuA.GetRowSegment(1, 2, 5).AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(r, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixGetColumnSegment()
        {
            var a = _cpu.CreateMatrix(9, 8, (x, y) => (x + 1) * (y + 1)).AsIndexable();
            var r = a.GetColumnSegment(1, 2, 5).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                gpuResults = gpuA.GetColumnSegment(1, 2, 5).AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(r, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixConstrain()
        {
            var distribution = new Normal(0, 5);
            var a = _cpu.CreateMatrix(100, 100, (x, y) => Convert.ToSingle(distribution.Sample())).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                gpuA.Constrain(-2f, 2f);
                gpuResults = gpuA.AsIndexable();
            }

            a.Constrain(-2f, 2f);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void TestIdentity()
        {
            var a = _cpu.CreateIdentityMatrix(1000).AsIndexable();

            IIndexableFloatMatrix a2;
            using (var gpuA = _cuda.CreateIdentityMatrix(1000))
                a2 = gpuA.AsIndexable();
            FloatMath.AreApproximatelyEqual(a, a2).Should().BeTrue();
        }

        [Fact]
        public void MatrixSvd()
        {
            var a = _cpu.CreateZeroMatrix(2, 2).AsIndexable();
            a[0, 0] = 4;
            a[0, 1] = 7;
            a[1, 0] = 2;
            a[1, 1] = 6;

            var svd = a.Svd();
            var cpuU = svd.U.AsIndexable();
            var cpuVT = svd.VT.AsIndexable();
            var cpuS = svd.S.AsIndexable();

            IIndexableFloatMatrix gpuU, gpuVT;
            IIndexableFloatVector gpuS;
            using (var gpuA = _cuda.CreateMatrix(a)) {
                var gpuSvd = gpuA.Svd();
                gpuU = gpuSvd.U.AsIndexable();
                gpuVT = gpuSvd.VT.AsIndexable();
                gpuS = gpuSvd.S.AsIndexable();
            }

            FloatMath.AreApproximatelyEqual(cpuU, gpuU).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(cpuVT, gpuVT).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(cpuS, gpuS).Should().BeTrue();
        }

        [Fact]
        public void MatrixToVector()
        {
            var matrix = _cpu.CreateMatrix(3, 4, (x, y) => (x + 1) * (y + 1)).AsIndexable();
            var vector = matrix.ReshapeAsVector();
            var matrix2 = vector.ReshapeAsMatrix(3, 4);
            FloatMath.AreApproximatelyEqual(matrix.AsIndexable(), matrix2.AsIndexable());

            using var gpuMatrix = _cuda.CreateMatrix(matrix.AsIndexable());
            using var gpuVector = gpuMatrix.ReshapeAsVector();
            using var gpuMatrix2 = gpuVector.ReshapeAsMatrix(3, 4);
            FloatMath.AreApproximatelyEqual(gpuMatrix.AsIndexable(), gpuMatrix2.AsIndexable()).Should().BeTrue();
        }
    }
}
