using BrightWire;
using BrightWire.Models;
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
            using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable()))
            using (var cpuVector = cpuTensor.ConvertToVector())
            using (var gpuVector = gpuTensor.ConvertToVector())
                FloatingPointHelper.AssertEqual(cpuVector.AsIndexable(), gpuVector.AsIndexable());
        }

        [TestMethod]
        public void TensorCreateFromVector()
        {
            const int DEPTH = 3, ROWS = 4, COLUMNS = 4;
            var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(ROWS, COLUMNS, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList());
            var cpuVector = cpuTensor.ConvertToVector();
            var cpuTensor2 = cpuVector.ConvertTo3DTensor(ROWS, COLUMNS, DEPTH);
            FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), cpuTensor2.AsIndexable());

            using (var gpuVector = _cuda.CreateVector(cpuVector.AsIndexable()))
            using (var gpuTensor2 = gpuVector.ConvertTo3DTensor(ROWS, COLUMNS, DEPTH)) {
                FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorConvertToMatrix()
        {
            using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable()))
            using (var cpuMatrix = cpuTensor.ConvertToMatrix())
            using (var gpuMatrix = gpuTensor.ConvertToMatrix())
                FloatingPointHelper.AssertEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable());
        }

        [TestMethod]
        public void TensorCreateFromMatrix()
        {
            const int DEPTH = 3, ROWS = 4, COLUMNS = 4;
            var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(ROWS, COLUMNS, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList());
            var cpuMatrix = cpuTensor.ConvertToMatrix();
            var cpuTensor2 = cpuMatrix.ConvertTo3DTensor(ROWS, COLUMNS);
            FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), cpuTensor2.AsIndexable());

            using (var gpuMatrix = _cuda.CreateMatrix(cpuMatrix.AsIndexable()))
            using (var gpuTensor2 = gpuMatrix.ConvertTo3DTensor(ROWS, COLUMNS)) {
                FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorAddPadding()
        {
            using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.AddPadding(1))
            using (var gpuPadding = gpuTensor.AddPadding(1)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorRemovePadding()
        {
            using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.RemovePadding(1))
            using (var gpuPadding = gpuTensor.RemovePadding(1)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorAddPadding2()
        {
            using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable()))
            using (var cpuPadding = cpuTensor.AddPadding(2))
            using (var gpuPadding = gpuTensor.AddPadding(2)) {
                FloatingPointHelper.AssertEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorIm2Col()
        {
            var normalDistribution = new Normal(0, 1);
            using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()))
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable()))
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
            using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 1).Select(i => _cpu.CreateMatrix(8, 8, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()))
            //using (var cpuTensor = _cpu.CreateTensor(Enumerable.Range(0, 3).Select(i => _cpu.Create(8, 8, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToList()))
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable()))
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

        //[TestMethod]
        //public void TensorCalculatePreviousError()
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
        //    var cpuUpdate = outputTensor.CalculatePreviousError(cpuFilter, INPUT_HEIGHT, INPUT_WIDTH, DEPTH, 0, FILTER_HEIGHT, FILTER_WIDTH, STRIDE);

        //    using (var gpuTensor = _cuda.CreateTensor(outputTensor.AsIndexable()))
        //    using (var gpuFIlter = _cuda.CreateMatrix(cpuFilter.AsIndexable())) {
        //        var gpuUpdate = gpuTensor.CalculatePreviousError(gpuFIlter, INPUT_HEIGHT, INPUT_WIDTH, DEPTH, 0, FILTER_HEIGHT, FILTER_WIDTH, STRIDE);

        //        FloatingPointHelper.AssertEqual(cpuUpdate.AsIndexable(), gpuUpdate.AsIndexable());
        //    }
        //}

        //[TestMethod]
        //public void TensorCalculatePreviousError2()
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
        //    var cpuUpdate = outputTensor.CalculatePreviousError(cpuFilter, INPUT_HEIGHT, INPUT_WIDTH, DEPTH, 0, FILTER_HEIGHT, FILTER_WIDTH, STRIDE);

        //    var cpuFilterList = new List<IReadOnlyList<IVector>>();
        //    for (var i = 0; i < cpuFilter.ColumnCount; i++)
        //        cpuFilterList.Add(cpuFilter.Column(i).Split(DEPTH).Select(v => v.Rotate(v.Count / FILTER_WIDTH)).ToList());

        //    var reverseIm2Col = outputTensor.ReverseIm2Col(cpuFilterList, INPUT_HEIGHT, INPUT_WIDTH, DEPTH, 0, FILTER_HEIGHT, FILTER_WIDTH, STRIDE);
        //    var cpuUpdate2 = _cpu.CreateTensor(reverseIm2Col, INPUT_HEIGHT, INPUT_WIDTH);
        //    FloatingPointHelper.AssertEqual(cpuUpdate.AsIndexable(), cpuUpdate2.AsIndexable());
        //}

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
                var slice = maxPool.GetMatrixAt(z).AsIndexable();
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
            var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 2).Select(i => _cpu.CreateZeroMatrix(INPUT_HEIGHT, INPUT_WIDTH)).ToList()).AsIndexable();
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

            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor)) {
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
            var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(INPUT_HEIGHT, INPUT_WIDTH, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()).AsIndexable();

            (var cpuMaxPool, var cpuIndex) = cpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, true);
            var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(INPUT_HEIGHT, INPUT_WIDTH, cpuIndex).AsIndexable();
            _AssertValuesAreInSamePlace(cpuReverseMaxPool, cpuTensor);

            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor)) {
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
            var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(INPUT_HEIGHT, INPUT_WIDTH, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList()).AsIndexable();

            (var cpuMaxPool, var cpuIndex) = cpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, false);
            using (var gpuTensor = _cuda.Create3DTensor(cpuTensor)) {
                (var gpuMaxPool, var gpuIndex) = gpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, STRIDE, false);
                FloatingPointHelper.AssertEqual(gpuMaxPool.AsIndexable(), cpuMaxPool.AsIndexable());
            }
        }

        void _TensorReverseIm2Col(int filterWidth, int filterHeight, int stride, int depth, int filterCount, int inputWidth, int inputHeight)
        {
            var normalDistribution = new Normal(0, 1);
            var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, depth).Select(i => _cpu.CreateMatrix(inputHeight, inputWidth, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList());
            var im2Col = cpuTensor.Im2Col(filterWidth, filterHeight, stride);
            var cpuFilter = _cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (i, j) => (float)normalDistribution.Sample());
            var output = im2Col.Multiply(cpuFilter);

            var matrixList = new List<IMatrix>();
            var newWidth = ((inputWidth - filterWidth) / stride) + 1;
            var newHeight = ((inputHeight - filterHeight) / stride) + 1;
            for (var i = 0; i < output.ColumnCount; i++)
                matrixList.Add(output.Column(i).ConvertInPlaceToMatrix(newWidth, newHeight));
            var outputTensor = _cpu.Create3DTensor(matrixList);

            var cpuFilterList = new List<IReadOnlyList<IVector>>();
            for (var i = 0; i < cpuFilter.ColumnCount; i++)
                cpuFilterList.Add(cpuFilter.Column(i).Split(depth).Select(v => v.Rotate(v.Count / filterWidth)).ToList());

            var cpuReverseIm2Col = outputTensor.ReverseIm2Col(cpuFilterList, inputHeight, inputWidth, depth, 0, filterHeight, filterWidth, stride);
            var cpuUpdate = cpuReverseIm2Col.ConvertTo3DTensor(inputHeight, inputWidth);

            using (var gpuTensor = _cuda.Create3DTensor(outputTensor.AsIndexable())) {
                var gpuFilterList = cpuFilterList.Select(fl => fl.Select(f => _cuda.CreateVector(f.AsIndexable())).ToList()).ToList();
                using (var gpuReverseIm2Col = gpuTensor.ReverseIm2Col(gpuFilterList, inputHeight, inputWidth, depth, 0, filterHeight, filterWidth, stride)) {
                    FloatingPointHelper.AssertEqual(gpuReverseIm2Col.AsIndexable(), cpuReverseIm2Col.AsIndexable());
                }
            }
        }

        [TestMethod]
        public void TensorReverseIm2Col()
        {
            _TensorReverseIm2Col(2, 2, 2, 1, 1, 4, 4);
        }

        [TestMethod]
        public void TensorReverseIm2Col2()
        {
            _TensorReverseIm2Col(2, 2, 2, 1, 2, 4, 4);
        }

        [TestMethod]
        public void TensorReverseIm2Col3()
        {
            _TensorReverseIm2Col(2, 2, 2, 2, 1, 4, 4);
        }

        [TestMethod]
        public void TensorReverseIm2Col4()
        {
            _TensorReverseIm2Col(2, 2, 2, 2, 2, 4, 4);
        }

        FloatMatrix _CreateMatrix(int depth, int rows, int columns, Func<int, int, int, float> valueProvider)
        {
            return new FloatMatrix {
                Row = Enumerable.Range(0, rows).Select(i => new FloatVector {
                    Data = Enumerable.Range(0, columns).Select(j => valueProvider?.Invoke(i, j, depth) ?? 0f).ToArray()
                }).ToArray()
            };
        }

        FloatTensor _CreateTensor(int rows, int columns, int depth, Func<int, int, int, float> valueProvider)
        {
            return new FloatTensor {
                Matrix = Enumerable.Range(0, depth).Select(k => _CreateMatrix(k, rows, columns, valueProvider)).ToArray()
            };
        }

        [TestMethod]
        public void Tensor4DCreate()
        {
            const int ROWS = 3, COLUMNS = 4, DEPTH = 2, COUNT = 5;
            var data = Enumerable.Range(0, COUNT).Select(z => _CreateTensor(ROWS, COLUMNS, DEPTH, null)).ToList();
            for(var i = 0; i < COUNT; i++) {
                var item = data[i];
                for (var j = 0; j < DEPTH; j++)
                    item.Matrix[j].Row[1].Data[2] = (j + 1) * (i + 1);
            }
            var cpuTensor = _cpu.Create4DTensor(data).AsIndexable();
            var gpuTensor = _cuda.Create4DTensor(data).AsIndexable();
            FloatingPointHelper.AssertEqual(cpuTensor, gpuTensor);
        }

        [TestMethod]
        public void Tensor4DAddPadding()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => _CreateTensor(3, 4, 2, (i, j, k) => (i+1) * (j+1) * (k + 1))).ToList();

            var cpuTensor = _cpu.Create4DTensor(data);
            var cpuPadded = cpuTensor.AddPadding(1);
            using (var gpuTensor = _cuda.Create4DTensor(data))
            using (var gpuPadded = gpuTensor.AddPadding(1)) {
                FloatingPointHelper.AssertEqual(cpuPadded.AsIndexable(), gpuPadded.AsIndexable());
            }
        }

        [TestMethod]
        public void Tensor4DRemovePadding()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => _CreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToList();

            var cpuTensor = _cpu.Create4DTensor(data);
            var cpuPadded = cpuTensor.RemovePadding(1);
            using (var gpuTensor = _cuda.Create4DTensor(data))
            using (var gpuPadded = gpuTensor.RemovePadding(1)) {
                FloatingPointHelper.AssertEqual(cpuPadded.AsIndexable(), gpuPadded.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorCombineDepthSlices()
        {
            var tensor = _CreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            var cpuTensor = _cpu.Create3DTensor(tensor);
            var cpuCombined = cpuTensor.CombineDepthSlices();

            using (var gpuTensor = _cuda.Create3DTensor(tensor))
            using (var gpuCombined = gpuTensor.CombineDepthSlices())
                FloatingPointHelper.AssertEqual(cpuCombined.AsIndexable(), gpuCombined.AsIndexable());
        }

        [TestMethod]
        public void TensorAddInPlace()
        {
            var tensor = _CreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            var tensor2 = _CreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            var cpuTensor = _cpu.Create3DTensor(tensor);
            var cpuTensor2 = _cpu.Create3DTensor(tensor2);
            cpuTensor.AddInPlace(cpuTensor2);

            using (var gpuTensor = _cuda.Create3DTensor(tensor))
            using (var gpuTensor2 = _cuda.Create3DTensor(tensor2)) {
                gpuTensor.AddInPlace(gpuTensor2);
                FloatingPointHelper.AssertEqual(cpuTensor.AsIndexable(), gpuTensor.AsIndexable());
            }
        }

        [TestMethod]
        public void Tensor4DMaxPool()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => _CreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToList();

            var cpuTensor = _cpu.Create4DTensor(data);
            var cpuPooled = cpuTensor.MaxPool(2, 2, 2, false).Result;

            using (var gpuTensor = _cuda.Create4DTensor(data))
            using (var gpuPooled = gpuTensor.MaxPool(2, 2, 2, false).Result) {
                FloatingPointHelper.AssertEqual(cpuPooled.AsIndexable(), gpuPooled.AsIndexable());
            }
        }

        [TestMethod]
        public void Tensor4DReverseMaxPool()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => _CreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToList();

            var cpuTensor = _cpu.Create4DTensor(data);
            var cpuResult = cpuTensor.MaxPool(2, 2, 2, true);
            var cpuReverseMaxPool = cpuResult.Result.ReverseMaxPool(4, 4, cpuResult.Index);

            using (var gpuTensor = _cuda.Create4DTensor(data)) {
                var gpuResult = gpuTensor.MaxPool(2, 2, 2, true);
                using (var gpuReverseMaxPool = gpuResult.Result.ReverseMaxPool(4, 4, gpuResult.Index)) {
                    FloatingPointHelper.AssertEqual(cpuReverseMaxPool.AsIndexable(), gpuReverseMaxPool.AsIndexable());
                }
            }
        }

        [TestMethod]
        public void Tensor4DIm2Col()
        {
            var normalDistribution = new Normal(0, 1);
            var data = Enumerable.Range(0, 5)
                .Select(z => _CreateTensor(4, 4, 2, (i, j, k) => (float)normalDistribution.Sample())).ToList();
            var cpuTensor = _cpu.Create4DTensor(data);
            var cpuResult = cpuTensor.Im2Col(2, 2, 1);

            using (var gpuTensor = _cuda.Create4DTensor(data))
            using(var gpuResult = gpuTensor.Im2Col(2, 2, 1)) {
                FloatingPointHelper.AssertEqual(cpuResult.AsIndexable(), gpuResult.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorMultiplyMatrix()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => _CreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToList();
            var cpuTensor = _cpu.Create4DTensor(data);
            var cpuIm2Col = cpuTensor.Im2Col(2, 2, 1);
            var cpuFilter = _cpu.CreateMatrix(2 * 2 * 2, 5, (i, j) => (i+1) * (j+1));
            var cpuOutput = cpuIm2Col.Multiply(cpuFilter);

            using (var gpuIm2Col = _cuda.Create3DTensor(cpuIm2Col.AsIndexable()))
            using (var gpuFilter = _cuda.CreateMatrix(cpuFilter.AsIndexable()))
            using (var gpuOutput = gpuIm2Col.Multiply(gpuFilter)) {
                FloatingPointHelper.AssertEqual(cpuOutput.AsIndexable(), gpuOutput.AsIndexable());
            }
        }

        [TestMethod]
        public void TensorToVector()
        {
            var data = _CreateTensor(4, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            var tensor = _cpu.Create3DTensor(data);
            var raw = data.GetAsRaw();
            var vector = _cpu.CreateVector(raw);
            var tensor2 = vector.ConvertTo3DTensor(4, 3, 2);
            FloatingPointHelper.AssertEqual(tensor.AsIndexable(), tensor2.AsIndexable());
        }

        //void _Tensor4DReverseIm2Col(int rows, int columns, int depth, int count, int filterWidth, int filterHeight, int filterCount, int stride)
        //{
        //    var normalDistribution = new Normal(0, 1);
        //    var data = Enumerable.Range(0, count)
        //        .Select(z => _CreateTensor(rows, columns, depth, (i, j, k) => (float)normalDistribution.Sample())).ToList();
        //    var cpuTensor = _cpu.CreateTensor(data);
        //    var cpuIm2Col = cpuTensor.Im2Col(filterWidth, filterHeight, stride);
        //    var cpuFilter = _cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (i, j) => (float)normalDistribution.Sample());
        //    var cpuOutput = cpuIm2Col.Multiply(cpuFilter);

        //    var cpuFilterList = new List<IReadOnlyList<IVector>>();
        //    for (var i = 0; i < cpuFilter.ColumnCount; i++)
        //        cpuFilterList.Add(cpuFilter.Column(i).Split(depth).Select(v => v.Rotate(v.Count / filterWidth)).ToList());

        //    var cpuReverseIm2Col = cpuOutput.ReverseIm2Col(cpuFilterList, rows, columns, depth, 0, filterHeight, filterWidth, stride);
        //}
    }
}
