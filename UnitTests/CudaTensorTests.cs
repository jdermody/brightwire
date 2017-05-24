using BrightWire;
using MathNet.Numerics.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Helper;

namespace UnitTests
{
    [TestClass]
    public class CudaTensorTests
    {
        static ILinearAlgebraProvider _cuda;
        static ILinearAlgebraProvider _cpu;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            _cuda = BrightWireGpuProvider.CreateLinearAlgebra(false);
            _cpu = BrightWireProvider.CreateLinearAlgebra(false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _cuda.Dispose();
            _cpu.Dispose();
        }

        [TestMethod]
        public void TensorConvertToVector()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuVector = cpuTensor.ConvertToVector())
            using (var gpuVector = gpuTensor.ConvertToVector())
                FloatingPointHelper.AssertEqual(cpuVector.AsIndexable(), gpuVector.AsIndexable());
        }

        [TestMethod]
        public void TensorCreateFromVector()
        {
            const int DEPTH = 3, ROWS = 4, COLUMNS = 4;
            var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(ROWS, COLUMNS, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList());
            var cpuVector = cpuTensor.ConvertToVector();
            var cpuTensor2 = _cpu.CreateTensor(cpuVector, ROWS, COLUMNS, DEPTH);
            FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), cpuTensor2.AsIndexable());

            using (var gpuVector = _cuda.CreateVector(cpuVector.AsIndexable()))
            using (var gpuTensor2 = _cuda.CreateTensor(gpuVector, ROWS, COLUMNS, DEPTH)) {
                FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorConvertToMatrix()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuMatrix = cpuTensor.ConvertToMatrix())
            using (var gpuMatrix = gpuTensor.ConvertToMatrix())
                FloatingPointHelper.AssertEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable());
        }

        [TestMethod]
        public void TensorCreateFromMatrix()
        {
            const int DEPTH = 3, ROWS = 4, COLUMNS = 4;
            var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(ROWS, COLUMNS, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList());
            var cpuMatrix = cpuTensor.ConvertToMatrix();
            var cpuTensor2 = _cpu.CreateTensor(cpuMatrix, ROWS, COLUMNS);
            FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), cpuTensor2.AsIndexable());

            using (var gpuMatrix = _cuda.CreateMatrix(cpuMatrix.AsIndexable()))
            using (var gpuTensor2 = _cuda.CreateTensor(gpuMatrix, ROWS, COLUMNS)) {
                FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorAddPadding()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.AddPadding(1))
            using (var gpuPadding = gpuTensor.AddPadding(1)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorRemovePadding()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.RemovePadding(1))
            using (var gpuPadding = gpuTensor.RemovePadding(1)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorAddPadding2()
        {
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.AddPadding(2))
            using (var gpuPadding = gpuTensor.AddPadding(2)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorIm2Col()
        {
            var normalDistribution = new Normal(0, 1);
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuMatrix = cpuTensor.Im2Col(2, 2, 2))
            using (var gpuMatrix = gpuTensor.Im2Col(2, 2, 2)) {
                var cpu = cpuMatrix.AsIndexable();
                var gpu = gpuMatrix.AsIndexable();
                FloatingPointHelper.AssertEqual(cpu, gpu);
            }
        }

        [TestMethod]
        public void TensorIm2Col2()
        {
            var normalDistribution = new Normal(0, 1);
            using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 1).Select(i => _cpu.CreateMatrix(8, 8, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()))
            //using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.Create(8, 8, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor.AsIndexable()))
            using (var cpuMatrix = cpuTensor.Im2Col(2, 2, 2))
            using (var gpuMatrix = gpuTensor.Im2Col(2, 2, 2)) {
                var cpu = cpuMatrix.AsIndexable();
                var gpu = gpuMatrix.AsIndexable();
                FloatingPointHelper.AssertEqual(cpu, gpu);
            }
        }

        //[TestMethod]
        //public void TensorCalculateWeightUpdate()
        //{
        //    const int FILTER_WIDTH = 2, FILTER_HEIGHT = 2, STRIDE = 2, DEPTH = 3, FILTER_COUNT = 4, INPUT_WIDTH = 8, INPUT_HEIGHT = 8;
        //    var normalDistribution = new Normal(0, 1);
        //    var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(INPUT_HEIGHT, INPUT_WIDTH, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList());
        //    var im2Col = cpuTensor.Im2Col(FILTER_WIDTH, FILTER_HEIGHT, STRIDE);
        //    var cpuFilter = _cpu.CreateMatrix(DEPTH * FILTER_WIDTH * FILTER_HEIGHT, FILTER_COUNT, (i, j) => (float)normalDistribution.Sample());
        //    var output = im2Col.Multiply(cpuFilter);

        //    var matrixList = new List<IMatrix>();
        //    var newWidth = ((INPUT_WIDTH - FILTER_WIDTH) / STRIDE) + 1;
        //    var newHeight = ((INPUT_HEIGHT - FILTER_HEIGHT) / STRIDE) + 1;
        //    for (var i = 0; i < output.ColumnCount; i++)
        //        matrixList.Add(output.Column(i).ConvertInPlaceToMatrix(newWidth, newHeight));
        //    var outputTensor = _cpu.CreateTensor(matrixList);
        //    var cpuUpdate = outputTensor.CalculateWeightUpdate(im2Col);

        //    using (var gpuTensor = _cuda.CreateTensor(outputTensor.AsIndexable()))
        //    using (var gpuIm2Col = _cuda.CreateMatrix(im2Col.AsIndexable())) {
        //        var gpuUpdate = gpuTensor.CalculateWeightUpdate(gpuIm2Col);

        //        FloatingPointHelper.AssertEqual(cpuUpdate.BiasUpdate.AsIndexable(), gpuUpdate.BiasUpdate.AsIndexable());
        //        FloatingPointHelper.AssertEqual(cpuUpdate.WeightUpdate.AsIndexable(), gpuUpdate.WeightUpdate.AsIndexable());
        //    }
        //}

        [TestMethod]
        public void TensorCalculatePreviousError()
        {
            const int FILTER_WIDTH = 2, FILTER_HEIGHT = 2, STRIDE = 2, DEPTH = 3, FILTER_COUNT = 4, INPUT_WIDTH = 8, INPUT_HEIGHT = 8;
            var normalDistribution = new Normal(0, 1);
            var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(INPUT_HEIGHT, INPUT_WIDTH, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList());
            var im2Col = cpuTensor.Im2Col(FILTER_WIDTH, FILTER_HEIGHT, STRIDE);
            var cpuFilter = _cpu.CreateMatrix(DEPTH * FILTER_WIDTH * FILTER_HEIGHT, FILTER_COUNT, (i, j) => (float)normalDistribution.Sample());
            var output = im2Col.Multiply(cpuFilter);

            var matrixList = new List<IMatrix>();
            var newWidth = ((INPUT_WIDTH - FILTER_WIDTH) / STRIDE) + 1;
            var newHeight = ((INPUT_HEIGHT - FILTER_HEIGHT) / STRIDE) + 1;
            for (var i = 0; i < output.ColumnCount; i++)
                matrixList.Add(output.Column(i).ConvertInPlaceToMatrix(newWidth, newHeight));
            var outputTensor = _cpu.CreateTensor(matrixList);
            var cpuUpdate = outputTensor.CalculatePreviousError(cpuFilter, INPUT_HEIGHT, INPUT_WIDTH, DEPTH, 0, FILTER_HEIGHT, FILTER_WIDTH, STRIDE);

            using (var gpuTensor = _cuda.CreateTensor(outputTensor.AsIndexable()))
            using (var gpuFIlter = _cuda.CreateMatrix(cpuFilter.AsIndexable())) {
                var gpuUpdate = gpuTensor.CalculatePreviousError(gpuFIlter, INPUT_HEIGHT, INPUT_WIDTH, DEPTH, 0, FILTER_HEIGHT, FILTER_WIDTH, STRIDE);

                FloatingPointHelper.AssertEqual(cpuUpdate.AsIndexable(), gpuUpdate.AsIndexable());
            }
        }

        void _AssertAreSame(IReadOnlyList<(int[] X, int[] Y)> cpuIndex, IReadOnlyList<(int[] X, int[] Y)> gpuIndex)
        {
            Assert.AreEqual(cpuIndex.Count, gpuIndex.Count);
            for (var i = 0; i < cpuIndex.Count; i++) {
                var list1 = cpuIndex[i];
                var list2 = gpuIndex[i];
                Assert.AreEqual(list1.X.Length, list2.X.Length);
                Assert.AreEqual(list1.Y.Length, list2.Y.Length);
                for (var j = 0; j < list1.X.Length; j++) {
                    Assert.AreEqual(list1.X[j], list2.X[j]);
                    Assert.AreEqual(list1.Y[j], list2.Y[j]);
                }
            }
        }

        void _AssertValuesAreInSamePlace(IIndexable3DTensor maxPool, IIndexable3DTensor source)
        {
            for (var z = 0; z < maxPool.Depth; z++) {
                var slice = maxPool.GetDepthSlice(z).AsIndexable();
                for (var i = 0; i < slice.RowCount; i++) {
                    for (var j = 0; j < slice.ColumnCount; j++) {
                        var val = slice[i, j];
                        if (val != 0f) {
                            Assert.AreEqual(val, source[i, j, z]);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TensorMaxPool()
        {
            const int FILTER_WIDTH = 2, FILTER_HEIGHT = 2, STRIDE = 2, INPUT_WIDTH = 4, INPUT_HEIGHT = 4;
            var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 2).Select(i => _cpu.CreateMatrix(INPUT_HEIGHT, INPUT_WIDTH, 0f)).ToList()).AsIndexable();
            cpuTensor[0, 0, 0] = 1f;
            cpuTensor[0, 3, 0] = 2f;
            cpuTensor[3, 0, 0] = 3f;
            cpuTensor[3, 3, 0] = 4f;

            cpuTensor[1, 1, 1] = 1f;
            cpuTensor[1, 2, 1] = 2f;
            cpuTensor[2, 1, 1] = 3f;
            cpuTensor[2, 2, 1] = 4f;

            (var cpuMaxPool, var cpuIndex) = cpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, true);
            var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(INPUT_HEIGHT, INPUT_WIDTH, cpuIndex).AsIndexable();
            FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), cpuReverseMaxPool);

            using (var gpuTensor = _cuda.CreateTensor(cpuTensor)) {
                (var gpuMaxPool, var gpuIndex) = gpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, true);
                FloatingPointHelper.AssertEqual(gpuMaxPool.AsIndexable(), cpuMaxPool.AsIndexable());
                using (var gpuReverseMaxPool = gpuMaxPool.ReverseMaxPool(INPUT_HEIGHT, INPUT_WIDTH, gpuIndex)) {
                    FloatingPointHelper.AssertEqual(gpuReverseMaxPool.AsIndexable(), cpuReverseMaxPool);
                }
            }
        }

        [TestMethod]
        public void TensorMaxPool2()
        {
            const int FILTER_WIDTH = 2, FILTER_HEIGHT = 2, STRIDE = 2, DEPTH = 3, INPUT_WIDTH = 8, INPUT_HEIGHT = 8;
            var normalDistribution = new Normal(0, 1);
            var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(INPUT_HEIGHT, INPUT_WIDTH, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()).AsIndexable();

            (var cpuMaxPool, var cpuIndex) = cpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, true);
            var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(INPUT_HEIGHT, INPUT_WIDTH, cpuIndex).AsIndexable();
            _AssertValuesAreInSamePlace(cpuReverseMaxPool, cpuTensor);

            using (var gpuTensor = _cuda.CreateTensor(cpuTensor)) {
                (var gpuMaxPool, var gpuIndex) = gpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, true);
                FloatingPointHelper.AssertEqual(gpuMaxPool.AsIndexable(), cpuMaxPool.AsIndexable());
                using (var gpuReverseMaxPool = gpuMaxPool.ReverseMaxPool(INPUT_HEIGHT, INPUT_WIDTH, gpuIndex)) {
                    FloatingPointHelper.AssertEqual(gpuReverseMaxPool.AsIndexable(), cpuReverseMaxPool);
                }
            }
        }

        [TestMethod]
        public void TensorMaxPool3()
        {
            const int FILTER_WIDTH = 2, FILTER_HEIGHT = 2, STRIDE = 2, DEPTH = 3, INPUT_WIDTH = 8, INPUT_HEIGHT = 8;
            var normalDistribution = new Normal(0, 1);
            var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(INPUT_HEIGHT, INPUT_WIDTH, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()).AsIndexable();

            (var cpuMaxPool, var cpuIndex) = cpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, false);
            using (var gpuTensor = _cuda.CreateTensor(cpuTensor)) {
                (var gpuMaxPool, var gpuIndex) = gpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, false);
                FloatingPointHelper.AssertEqual(gpuMaxPool.AsIndexable(), cpuMaxPool.AsIndexable());
            }
        }
    }
}
