using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrightWire;
using BrightWire.LinearAlgebra;
using MathNet.Numerics.Distributions;
using System.IO;
using System.Text;
using UnitTests.Helper;

namespace UnitTests
{
    [TestClass]
    public class CudaLinearAlgebraTests
    {
        static ILinearAlgebraProvider _cuda;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _cuda = new CudaProvider();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _cuda.Dispose();
        }

        void _MatrixMultiplication(int rowsA, int columnsArowsB, int columnsB)
        {
            var lap = new NumericsProvider();
            var rand = new Random(1);
            var a = lap.Create(rowsA, columnsArowsB, (j, k) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var b = lap.Create(columnsArowsB, columnsB, (j, k) => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var cpuResults = a.Multiply(b);
            IIndexableMatrix gpuResults;

            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.Multiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
        }

        [TestMethod]
        public void MatrixMultiplication()
        {
            _MatrixMultiplication(5, 2, 5);
        }

        [TestMethod]
        public void MatrixMultiplication2()
        {
            _MatrixMultiplication(500, 200, 500);
        }

        [TestMethod]
        public void MatrixMultiplication3()
        {
            _MatrixMultiplication(5, 5, 5);
        }

        [TestMethod]
        public void MatrixMultiplication4()
        {
            _MatrixMultiplication(5, 10, 5);
        }

        [TestMethod]
        public void MatrixMultiplication5()
        {
            _MatrixMultiplication(5, 10, 2);
        }

        [TestMethod]
        public void MatrixMultiplication6()
        {
            _MatrixMultiplication(50, 10, 2);
        }

        [TestMethod]
        public void MatrixMultiplication7()
        {
            _MatrixMultiplication(2, 10, 20);
        }

        [TestMethod]
        public void MatrixMultiplication8()
        {
            _MatrixMultiplication(20, 1, 19);
        }

        void _Transpose(int rows, int columns)
        {
            var lap = new NumericsProvider();
            var a = lap.Create(rows, columns, (j, k) => k).AsIndexable();
            var aT = a.Transpose();
            IIndexableMatrix gpuResults;

            using (var gpuA = _cuda.Create(a))
            using (var gpuAT = gpuA.Transpose())
                gpuResults = gpuAT.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, aT.AsIndexable());
        }

        [TestMethod]
        public void MatrixTranspose()
        {
            _Transpose(2, 5);
        }

        [TestMethod]
        public void MatrixTranspose2()
        {
            _Transpose(5, 2);
        }

        [TestMethod]
        public void MatrixTranspose3()
        {
            _Transpose(500, 2);
        }

        [TestMethod]
        public void MatrixTranspose4()
        {
            _Transpose(2, 500);
        }

        [TestMethod]
        public void MatrixTranspose5()
        {
            _Transpose(20, 20);
        }

        [TestMethod]
        public void MatrixTranspose6()
        {
            _Transpose(500, 500);
        }

        [TestMethod]
        public void MatrixTransposeAndMultiplication()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var b = lap.Create(3, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.TransposeAndMultiply(b);
            IIndexableMatrix gpuResults;

            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.TransposeAndMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
        }

        [TestMethod]
        public void MatrixTransposeAndMultiplication2()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 6, (j, k) => k).AsIndexable();
            var b = lap.Create(2, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.TransposeThisAndMultiply(b);
            IIndexableMatrix gpuResults;

            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.TransposeThisAndMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
        }

        [TestMethod]
        public void MatrixAdd()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var b = lap.Create(2, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.Add(b);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.Add(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
        }

        [TestMethod]
        public void MatrixSubtract()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var b = lap.Create(2, 5, (j, k) => j).AsIndexable();

            IIndexableMatrix gpuResults, gpuResults2;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                var aStr = gpuA.ToString();
                var bStr = gpuB.ToString();
                using (var gpuC = gpuA.Subtract(gpuB))
                using (var gpuD = gpuB.Subtract(gpuA)) {
                    gpuResults = gpuC.AsIndexable();
                    gpuResults2 = gpuD.AsIndexable();
                }
                Assert.AreEqual(aStr, gpuA.ToString());
                Assert.AreEqual(bStr, gpuB.ToString());
            }

            var cpuResults = a.Subtract(b);
            var cpuResults2 = b.Subtract(a);
            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
            FloatingPointHelper.AssertEqual(gpuResults2, cpuResults2.AsIndexable());
        }

        [TestMethod]
        public void MatrixPointwiseMultiply()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var b = lap.Create(2, 5, (j, k) => j).AsIndexable();
            var cpuResults = a.PointwiseMultiply(b);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.PointwiseMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
        }

        [TestMethod]
        public void MatrixPointwiseDivide()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k + 1).AsIndexable();
            var b = lap.Create(2, 5, (j, k) => j + 1).AsIndexable();
            var cpuResults = a.PointwiseDivide(b);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.PointwiseDivide(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
        }

        [TestMethod]
        public void MatrixSqrt()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k + 1).AsIndexable();
            const float adjustment = 1e-8f;
            a[0, 0] = -adjustment;
            var cpuResults = a.Sqrt(adjustment);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuC = gpuA.Sqrt(1e-8f))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, cpuResults.AsIndexable());
        }

        [TestMethod]
        public void MatrixMultiplyScalar()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            const float scalar = 2.5f;

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.Multiply(scalar);
                gpuResults = gpuA.AsIndexable();
            }

            a.Multiply(scalar);
            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void VectorColumnMatrix()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i).AsIndexable();
            var matrix = a.ToColumnMatrix().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = gpuA.ToColumnMatrix())
                gpuResults = gpuB.AsIndexable();

            FloatingPointHelper.AssertEqual(matrix, gpuResults);
        }

        [TestMethod]
        public void VectorRowMatrix()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i).AsIndexable();
            var matrix = a.ToRowMatrix().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var m = gpuA.ToRowMatrix()) {
                gpuResults = m.AsIndexable();
            }

            FloatingPointHelper.AssertEqual(matrix, gpuResults);
        }

        [TestMethod]
        public void VectorAdd()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i).AsIndexable();
            var b = np.Create(5, i => i * 2).AsIndexable();
            var c = a.Add(b).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.Add(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(c, gpuResults);
        }

        [TestMethod]
        public void VectorSubtract()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i).AsIndexable();
            var b = np.Create(5, i => i * 2).AsIndexable();
            var c = a.Subtract(b).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.Subtract(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(c, gpuResults);
        }

        [TestMethod]
        public void VectorPointwiseMultiply()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i).AsIndexable();
            var b = np.Create(5, i => i * 2).AsIndexable();
            var c = a.PointwiseMultiply(b).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var gpuC = gpuA.PointwiseMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatingPointHelper.AssertEqual(c, gpuResults);
        }

        [TestMethod]
        public void VectorDotProduct()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i).AsIndexable();
            var b = np.Create(5, i => i * 2).AsIndexable();
            var dot1 = a.DotProduct(b);

            float dot2;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
                dot2 = gpuA.DotProduct(gpuB);

            Assert.AreEqual(dot1, dot2);
        }

        [TestMethod]
        public void VectorL2Norm()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i).AsIndexable();
            var res1 = a.L2Norm();

            float res2;
            using (var gpuA = _cuda.Create(a))
                res2 = gpuA.L2Norm();
            Assert.AreEqual(res1, res2);
        }

        [TestMethod]
        public void VectorMaximumIndex()
        {
            var np = new NumericsProvider();
            var a = np.Create(new[] { 1.0f, 2.0f, 1.0f, 1.0f }).AsIndexable();
            var res1 = a.MaximumIndex();

            int res2;
            using (var gpuA = _cuda.Create(a))
                res2 = gpuA.MaximumIndex();
            Assert.AreEqual(res1, res2);
        }

        [TestMethod]
        public void VectorMinimumIndex()
        {
            var np = new NumericsProvider();
            var a = np.Create(new[] { 3.0f, -2.0f, 1.0f, 2.0f }).AsIndexable();
            var res1 = a.MinimumIndex();

            int res2;
            using (var gpuA = _cuda.Create(a))
                res2 = gpuA.MinimumIndex();
            Assert.AreEqual(res1, res2);
        }

        [TestMethod]
        public void VectorAddInPlace()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i * 2).AsIndexable();
            var b = np.Create(5, i => i).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.AddInPlace(gpuB, 2.5f, 3.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddInPlace(b, 2.5f, 3.5f);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void VectorSubtractInPlace()
        {
            var np = new NumericsProvider();
            var a = np.Create(5, i => i * 2).AsIndexable();
            var b = np.Create(5, i => i).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.SubtractInPlace(gpuB, 2.5f, 3.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.SubtractInPlace(b, 2.5f, 3.5f);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void VectorSqrt()
        {
            var np = new NumericsProvider();
            var a = np.Create(10, i => i * 2).AsIndexable();
            var b = a.Sqrt().AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = gpuA.Sqrt()) {
                gpuResults = gpuB.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(b, gpuResults);
        }

        [TestMethod]
        public void VectorGetNewVectorFromIndices()
        {
            var np = new NumericsProvider();
            var a = np.Create(10, i => i * 2).AsIndexable();
            int[] array = new[] { 2, 3, 5 };
            var b = a.GetNewVectorFromIndexes(array).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = gpuA.GetNewVectorFromIndexes(array)) {
                gpuResults = gpuB.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(b, gpuResults);
        }

        [TestMethod]
        public void VectorCopyFrom()
        {
            var np = new NumericsProvider();
            var a = np.Create(10, i => i * 2).AsIndexable();
            var b = np.Create(10, 0).AsIndexable();
            b.CopyFrom(a);
            FloatingPointHelper.AssertEqual(a, b);

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(10, 0)) {
                gpuB.CopyFrom(gpuA);
                gpuResults = gpuB.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void MatrixColumn()
        {
            const int INDEX = 1;
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => j * k).AsIndexable();
            var row = a.Column(INDEX).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuCol = gpuA.Column(INDEX))
                gpuResults = gpuCol.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, row);
        }

        [TestMethod]
        public void MatrixRow()
        {
            const int INDEX = 1;
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k * j).AsIndexable();
            var row = a.Row(INDEX).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuRow = gpuA.Row(INDEX))
                gpuResults = gpuRow.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, row);
        }

        [TestMethod]
        public void MatrixRowSums()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var rowSums = a.RowSums().AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuRowSums = gpuA.RowSums())
                gpuResults = gpuRowSums.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, rowSums);
        }

        [TestMethod]
        public void MatrixColumnSums()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var colSums = a.ColumnSums().AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuColSums = gpuA.ColumnSums())
                gpuResults = gpuColSums.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, colSums);
        }

        [TestMethod]
        public void MatrixAddInPlace()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var b = lap.Create(2, 5, (j, k) => j).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.AddInPlace(gpuB, 1.5f, 2.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddInPlace(b, 1.5f, 2.5f);
            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void MatrixSubtractInPlace()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k).AsIndexable();
            var b = lap.Create(2, 5, (j, k) => j).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.SubtractInPlace(gpuB, 1.5f, 2.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.SubtractInPlace(b, 1.5f, 2.5f);
            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void MatrixAddToEachRow()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k * j).AsIndexable();
            var b = lap.Create(5, (i) => i).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.AddToEachRow(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddToEachRow(b);
            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void MatrixAddToEachColumn()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(2, 5, (j, k) => k * j).AsIndexable();
            var b = lap.Create(2, (i) => i + 5).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.AddToEachColumn(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddToEachColumn(b);
            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void MatrixSigmoidActivation()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new MathNet.Numerics.Distributions.Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.SigmoidActivation().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var sigmoid = gpuA.SigmoidActivation())
                gpuResults = sigmoid.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixSigmoidDerivative()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new MathNet.Numerics.Distributions.Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.SigmoidDerivative().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var sigmoid = gpuA.SigmoidDerivative())
                gpuResults = sigmoid.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixTanhActivation()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new MathNet.Numerics.Distributions.Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.TanhActivation().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var tanh = gpuA.TanhActivation())
                gpuResults = tanh.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixTanhDerivative()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new MathNet.Numerics.Distributions.Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.TanhDerivative().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var tanh = gpuA.TanhDerivative())
                gpuResults = tanh.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixRELUActivation()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new MathNet.Numerics.Distributions.Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.ReluActivation().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var relu = gpuA.ReluActivation())
                gpuResults = relu.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixRELUDerivative()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new MathNet.Numerics.Distributions.Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.ReluDerivative().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var relu = gpuA.ReluDerivative())
                gpuResults = relu.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixLeakyRELUActivation()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.LeakyReluActivation().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var relu = gpuA.LeakyReluActivation())
                gpuResults = relu.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixLeakyRELUDerivative()
        {
            var lap = new NumericsProvider();
            var normalDistribution = new Normal(0, 1);
            var a = lap.Create(10, 10, (j, k) => Convert.ToSingle(normalDistribution.Sample())).AsIndexable();
            var b = a.LeakyReluDerivative().AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var relu = gpuA.LeakyReluDerivative())
                gpuResults = relu.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void MatrixNewMatrixFromRows()
        {
            var lap = new NumericsProvider();
            var rows = new[] { 1, 2 };
            var a = lap.Create(3, 2, (j, k) => k + 1).AsIndexable();
            var results = a.GetNewMatrixFromRows(rows).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var m = gpuA.GetNewMatrixFromRows(rows))
                gpuResults = m.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, results);
        }

        [TestMethod]
        public void MatrixNewMatrixFromColumns()
        {
            var lap = new NumericsProvider();
            var cols = new[] { 1, 2 };
            var a = lap.Create(5, 5, (j, k) => k + 1).AsIndexable();
            var results = a.GetNewMatrixFromColumns(cols).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var m = gpuA.GetNewMatrixFromColumns(cols))
                gpuResults = m.AsIndexable();

            FloatingPointHelper.AssertEqual(gpuResults, results);
        }

        [TestMethod]
        public void MatrixClearRows()
        {
            var lap = new NumericsProvider();
            var rows = new[] { 1, 2 };
            var a = lap.Create(3, 2, (j, k) => k + 1).AsIndexable();
            a.ClearRows(rows);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                a.ClearRows(rows);
                gpuResults = a.AsIndexable();
            }

            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void MatrixClearColumns()
        {
            var lap = new NumericsProvider();
            var cols = new[] { 1, 2 };
            var a = lap.Create(5, 5, (j, k) => k + 1).AsIndexable();
            a.ClearColumns(cols);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.ClearColumns(cols);
                gpuResults = gpuA.AsIndexable();
            }

            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void MatrixClear()
        {
            var lap = new NumericsProvider();
            var cols = new[] { 1, 2 };
            var a = lap.Create(5, 5, (j, k) => k + 1).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.Clear();
                gpuResults = gpuA.AsIndexable();
            }

            a.Clear();
            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void MatrixClone()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(5, 5, (j, k) => k + 1).AsIndexable();
            var b = a.Clone().AsIndexable();
            FloatingPointHelper.AssertEqual(a, b);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var clone = gpuA.Clone()) {
                gpuResults = clone.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void VectorClone()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(5, i => i).AsIndexable();
            var b = a.Clone().AsIndexable();
            FloatingPointHelper.AssertEqual(a, b);

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var clone = gpuA.Clone()) {
                gpuResults = clone.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(gpuResults, b);
        }

        [TestMethod]
        public void VectorMultiply()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(5, i => i).AsIndexable();
            const float OPERAND = 2f;

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.Multiply(OPERAND);
                gpuResults = gpuA.AsIndexable();
            }

            a.Multiply(OPERAND);
            FloatingPointHelper.AssertEqual(gpuResults, a);
        }

        [TestMethod]
        public void VectorReadWrite()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(5, i => i).AsIndexable();

            // test Numerics -> Numerics serialisation
            var serialised = a.Data;
            var b = lap.CreateVector(serialised);
            FloatingPointHelper.AssertEqual(a.AsIndexable(), b.AsIndexable());

            // test Numerics -> Cuda serialisation
            using (var c = _cuda.CreateVector(serialised)) {
                FloatingPointHelper.AssertEqual(a.AsIndexable(), c.AsIndexable());

                // test Cuda -> Cuda serialisation
                var serialised2 = c.Data;
                using (var d = _cuda.CreateVector(serialised2))
                    FloatingPointHelper.AssertEqual(a.AsIndexable(), d.AsIndexable());

                // test Cuda -> Numerics serialisation
                var e = lap.CreateVector(c.Data);
                FloatingPointHelper.AssertEqual(a.AsIndexable(), e.AsIndexable());
            }
        }

        [TestMethod]
        public void MatrixReadWrite()
        {
            var lap = new NumericsProvider();
            var a = lap.Create(7, 20, (x, y) => x * 10 + y).AsIndexable();

            // test Numerics -> Numerics serialisation
            var serialised = a.Data;
            var b = lap.CreateMatrix(serialised);
            FloatingPointHelper.AssertEqual(a.AsIndexable(), b.AsIndexable());

            // test Numerics -> Cuda serialisation
            using (var c = _cuda.CreateMatrix(serialised)) {
                FloatingPointHelper.AssertEqual(a.AsIndexable(), c.AsIndexable());

                // test Cuda -> Cuda serialisation
                var serialised2 = c.Data;
                using (var d = _cuda.CreateMatrix(serialised2))
                    FloatingPointHelper.AssertEqual(a.AsIndexable(), d.AsIndexable());

                // test Cuda -> Numerics serialisation
                var e = lap.CreateMatrix(c.Data);
                FloatingPointHelper.AssertEqual(a.AsIndexable(), e.AsIndexable());
            }
        }

        [TestMethod]
        public void MatrixConcatColumns()
        {
            var lap = new NumericsProvider();
            var rand = new Random();
            var a = lap.CreateIndexable(4000, 300, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var b = lap.CreateIndexable(200, 300, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var c = a.ConcatColumns(b).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var concat = gpuA.ConcatColumns(gpuB)) {
                gpuResults = concat.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(c, gpuResults);
        }

        [TestMethod]
        public void MatrixConcatRows()
        {
            var lap = new NumericsProvider();
            var rand = new Random();
            var a = lap.CreateIndexable(300, 4000, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var b = lap.CreateIndexable(300, 200, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var c = a.ConcatRows(b).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
            using (var concat = gpuA.ConcatRows(gpuB)) {
                gpuResults = concat.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(c, gpuResults);
        }

        [TestMethod]
        public void MatrixSplitColumns()
        {
            const int POSITION = 2000;
            var lap = new NumericsProvider();
            var rand = new Random();
            var a = lap.CreateIndexable(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var r = a.SplitColumns(POSITION);

            IIndexableMatrix gpuResults1, gpuResults2;
            using (var gpuA = _cuda.Create(a)) {
                var r2 = gpuA.SplitColumns(POSITION);
                using (var m1 = r2.Item1)
                using (var m2 = r2.Item2) {
                    gpuResults1 = m1.AsIndexable();
                    gpuResults2 = m2.AsIndexable();
                }
            }
            FloatingPointHelper.AssertEqual(gpuResults1, r.Item1.AsIndexable());
            FloatingPointHelper.AssertEqual(gpuResults2, r.Item2.AsIndexable());
        }

        [TestMethod]
        public void MatrixSplitRows()
        {
            const int POSITION = 2000;
            var lap = new NumericsProvider();
            var rand = new Random();
            var a = lap.CreateIndexable(6000, 3000, (x, y) => Convert.ToSingle(rand.NextDouble()));
            var r = a.SplitRows(POSITION);

            IIndexableMatrix gpuResults1, gpuResults2;
            using (var gpuA = _cuda.Create(a)) {
                var r2 = gpuA.SplitRows(POSITION);
                using (var m1 = r2.Item1)
                using (var m2 = r2.Item2) {
                    gpuResults1 = m1.AsIndexable();
                    gpuResults2 = m2.AsIndexable();
                }
            }
            FloatingPointHelper.AssertEqual(gpuResults1, r.Item1.AsIndexable());
            FloatingPointHelper.AssertEqual(gpuResults2, r.Item2.AsIndexable());
        }

        [TestMethod]
        public void MatrixL1Regularisation()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(6, 3, (x, y) => x * 2 + y);
            const float OPERAND = 2f;

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.L1Regularisation(OPERAND);
                gpuResults = gpuA.AsIndexable();
            }
            a.L1Regularisation(OPERAND);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void MatrixColumnL2Norm()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(6, 3, (x, y) => x * 2 + y);
            var r = a.ColumnL2Norm().AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var norm = gpuA.ColumnL2Norm()) {
                gpuResults = norm.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(r, gpuResults);
        }

        [TestMethod]
        public void MatrixRowL2Norm()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(6, 3, (x, y) => x * 2 + y);
            var r = a.RowL2Norm().AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var norm = gpuA.RowL2Norm()) {
                gpuResults = norm.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(r, gpuResults);
        }

        [TestMethod]
        public void MatrixPointwiseDivideRows()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(6, 3, (x, y) => x * 2 + y);
            var b = lap.CreateIndexable(6, i => i);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.PointwiseDivideRows(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.PointwiseDivideRows(b);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void MatrixPointwiseDivideColumns()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(6, 3, (x, y) => x * 2 + y);
            var b = lap.CreateIndexable(3, i => i);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b)) {
                gpuA.PointwiseDivideColumns(gpuB);
                gpuResults = gpuA.AsIndexable();
            }

            a.PointwiseDivideColumns(b);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void MatrixDiagonal()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(6, 6, (x, y) => x * 2 + y);
            var d = a.Diagonal().AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var diagonal = gpuA.Diagonal()) {
                gpuResults = diagonal.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(d, gpuResults);
        }

        [TestMethod]
        public void MatrixPow()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(6, 3, (x, y) => x * 2 + y);
            const float OPERAND = 2.5f;
            var r = a.Pow(OPERAND).AsIndexable();

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a))
            using (var pow = gpuA.Pow(OPERAND)) {
                gpuResults = pow.AsIndexable();
            }
            FloatingPointHelper.AssertEqual(r, gpuResults);
        }

        [TestMethod]
        public void MatrixUpdateRow()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(10, 10, (x, y) => x * 2 + y);
            var r = lap.CreateIndexable(2, x => -1f);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.UpdateRow(2, r, 3);
                gpuResults = gpuA.AsIndexable();
            }

            a.UpdateRow(2, r, 3);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void MatrixUpdateColumn()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(10, 10, (x, y) => x * 2 + y);
            var r = lap.CreateIndexable(2, x => -1f);

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.UpdateColumn(2, r, 3);
                gpuResults = gpuA.AsIndexable();
            }

            a.UpdateColumn(2, r, 3);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void MatrixGetRowSegment()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(10, 10, (x, y) => x * 2 + y);
            var r = a.GetRowSegment(1, 2, 5).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuResults = gpuA.GetRowSegment(1, 2, 5).AsIndexable();
            }
            FloatingPointHelper.AssertEqual(r, gpuResults);
        }

        [TestMethod]
        public void MatrixGetColumnSegment()
        {
            var lap = new NumericsProvider();
            var a = lap.CreateIndexable(10, 10, (x, y) => x * 2 + y);
            var r = a.GetColumnSegment(1, 2, 5).AsIndexable();

            IIndexableVector gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuResults = gpuA.GetColumnSegment(1, 2, 5).AsIndexable();
            }
            FloatingPointHelper.AssertEqual(r, gpuResults);
        }

        [TestMethod]
        public void MatrixConstrain()
        {
            var lap = new NumericsProvider();
            var distribution = new Normal(0, 5);
            var a = lap.CreateIndexable(100, 100, (x, y) => Convert.ToSingle(distribution.Sample()));

            IIndexableMatrix gpuResults;
            using (var gpuA = _cuda.Create(a)) {
                gpuA.Constrain(-2f, 2f);
                gpuResults = gpuA.AsIndexable();
            }

            a.Constrain(-2f, 2f);
            FloatingPointHelper.AssertEqual(a, gpuResults);
        }

        [TestMethod]
        public void VectorEuclideanDistance()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.EuclideanDistance(b);

            float distance2;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
                distance2 = gpuA.EuclideanDistance(gpuB);

            Assert.IsTrue(FloatingPointHelper.AlmostEqual2sComplement(distance, distance2, 10));
        }

        [TestMethod]
        public void VectorCosineDistance()
        {
            var np = new NumericsProvider();
            var rand = new Random(0);

            var a = np.Create(5000, i => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var b = np.Create(5000, i => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var distance = a.CosineDistance(b);

            float distance2;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
                distance2 = gpuA.CosineDistance(gpuB);

            Assert.IsTrue(FloatingPointHelper.AlmostEqual2sComplement(distance, distance2, 10));
        }

        [TestMethod]
        public void VectorManhattanDistance()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.ManhattanDistance(b);

            float distance2;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
                distance2 = gpuA.ManhattanDistance(gpuB);

            Assert.IsTrue(FloatingPointHelper.AlmostEqual2sComplement(distance, distance2, 10));
        }

        [TestMethod]
        public void VectorMeanSquaredDistance()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.MeanSquaredDistance(b);

            float distance2;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
                distance2 = gpuA.MeanSquaredDistance(gpuB);

            Assert.IsTrue(FloatingPointHelper.AlmostEqual2sComplement(distance, distance2, 11));
        }

        [TestMethod]
        public void VectorSquaredEuclideanDistance()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.SquaredEuclidean(b);

            float distance2;
            using (var gpuA = _cuda.Create(a))
            using (var gpuB = _cuda.Create(b))
                distance2 = gpuA.SquaredEuclidean(gpuB);

            Assert.IsTrue(FloatingPointHelper.AlmostEqual2sComplement(distance, distance2, 11));
        }

        [TestMethod]
        public void VectorMinMax()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var minMax = a.GetMinMax();

            Tuple<float, float> minMax2;
            using (var gpuA = _cuda.Create(a))
                minMax2 = gpuA.GetMinMax();

            FloatingPointHelper.AssertEqual(minMax.Item1, minMax2.Item1);
            FloatingPointHelper.AssertEqual(minMax.Item2, minMax2.Item2);
        }

        [TestMethod]
        public void VectorAverage()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var average = a.Average();

            float average2;
            using (var gpuA = _cuda.Create(a))
                average2 = gpuA.Average();

            FloatingPointHelper.AssertEqual(average, average2, 7);
        }

        [TestMethod]
        public void VectorL1Norm()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var v1 = a.L1Norm();

            float v2;
            using (var gpuA = _cuda.Create(a))
                v2 = gpuA.L1Norm();

            FloatingPointHelper.AssertEqual(v1, v2);
        }

        [TestMethod]
        public void VectorAbs()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var v1 = a.Abs().AsIndexable();

            IIndexableVector v2;
            using (var gpuA = _cuda.Create(a))
                v2 = gpuA.Abs().AsIndexable();

            FloatingPointHelper.AssertEqual(v1, v2);
        }

        [TestMethod]
        public void VectorStdDev()
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var stdDev = a.StdDev(null);

            float stdDev2;
            using (var gpuA = _cuda.Create(a))
                stdDev2 = gpuA.StdDev(null);

            FloatingPointHelper.AssertEqual(stdDev, stdDev2);
        }

        void _TestNormalise(NormalisationType type)
        {
            var np = new NumericsProvider();
            var distribution = new Normal(0, 5);

            IIndexableVector v2;
            var a = np.Create(5000, i => Convert.ToSingle(distribution.Sample()));
            using (var gpuA = _cuda.Create(a.AsIndexable())) {
                gpuA.Normalise(type);
                v2 = gpuA.AsIndexable();
            }
            a.Normalise(type);
            var v1 = a.AsIndexable();
            FloatingPointHelper.AssertEqual(v1, v2, 12);
        }

        [TestMethod]
        public void VectorFeatureScaleNormalise()
        {
            _TestNormalise(NormalisationType.FeatureScale);
        }

        [TestMethod]
        public void VectorStandardNormalise()
        {
            _TestNormalise(NormalisationType.Standard);
        }

        [TestMethod]
        public void VectorManhattanNormalise()
        {
            _TestNormalise(NormalisationType.Manhattan);
        }

        [TestMethod]
        public void VectorEuclideanNormalise()
        {
            _TestNormalise(NormalisationType.Euclidean);
        }
    }
}
