using System;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;
using BrightWire.ExecutionGraph.Helper;

namespace BrightData.UnitTests
{
    public class TensorTests : CudaBase
    {
        static ITensor3D Apply(LinearAlgebraProvider lap, ITensor3D a, Func<ITensor3D, ITensor3D> func)
        {
            using var otherA = lap.CreateTensor3D(a);
            var otherB = func(otherA);
            return otherB;
        }

        static IMatrix Apply(LinearAlgebraProvider lap, ITensor3D a, Func<ITensor3D, IMatrix> func)
        {
            using var otherA = lap.CreateTensor3D(a);
            var otherB = func(otherA);
            return otherB;
        }

        static ITensor4D Apply(LinearAlgebraProvider lap, ITensor4D a, Func<ITensor4D, ITensor4D> func)
        {
            using var otherA = lap.CreateTensor4D(a);
            var otherB = func(otherA);
            return otherB;
        }

        [Fact]
		public void TensorConvertToVector()
        {
            using var cpuTensor = _cpu.CreateTensor3DAndThenDisposeInput(3.AsRange().Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var gpuTensor = _cuda.CreateTensor3D(cpuTensor.Depth, cpuTensor.RowCount, cpuTensor.ColumnCount, cpuTensor.Segment);
            using var mklTensor = _mkl.CreateTensor3D(cpuTensor.Depth, cpuTensor.RowCount, cpuTensor.ColumnCount, cpuTensor.Segment);

            using var cpu = cpuTensor.Reshape();
            using var gpu = gpuTensor.Reshape();
            using var mkl = mklTensor.Reshape();
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorCreateFromVector()
        {
            const int depth = 3, rows = 4, columns = 4;
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, depth).Select(i => _cpu.CreateMatrix(rows, columns, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var cpuVector = cpuTensor.Reshape();
            using var cpu = cpuVector.Reshape(rows, columns, depth);

            using var gpuVector = _cuda.CreateVector(cpuVector);
            using var gpu = gpuVector.Reshape(rows, columns, depth);

            using var mklVector = _mkl.CreateVector(cpuVector);
            using var mkl = mklVector.Reshape(rows, columns, depth);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorConvertToMatrix()
        {
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var gpuTensor = _cuda.CreateTensor3D(cpuTensor);
            using var mklTensor = _mkl.CreateTensor3D(cpuTensor);
            using var cpu = cpuTensor.ReshapeAsMatrix();
            using var gpu = gpuTensor.ReshapeAsMatrix();
            using var mkl = mklTensor.ReshapeAsMatrix();
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorCreateFromMatrix()
        {
            const int depth = 3, rows = 4, columns = 4;
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, depth).Select(i => _cpu.CreateMatrix(rows, columns, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var cpuMatrix = cpuTensor.ReshapeAsMatrix();
            using var cpu = cpuMatrix.Reshape(null, rows, columns);
            using var gpuMatrix = _cuda.CreateMatrix(cpuMatrix);
            using var gpu = gpuMatrix.Reshape(null, rows, columns);
            using var mklMatrix = _mkl.CreateMatrix(cpuMatrix);
            using var mkl = mklMatrix.Reshape(null, rows, columns);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorAddPadding()
        {
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var cpu = cpuTensor.AddPadding(1);
            using var gpu = Apply(_cuda, cpuTensor, a => a.AddPadding(1));
            using var mkl = Apply(_mkl, cpuTensor, a => a.AddPadding(1));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorRemovePadding()
        {
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var cpu = cpuTensor.RemovePadding(1);
            using var gpu = Apply(_cuda, cpuTensor, a => a.RemovePadding(1));
            using var mkl = Apply(_mkl, cpuTensor, a => a.RemovePadding(1));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorAddPadding2()
        {
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var cpu = cpuTensor.AddPadding(2);
            using var gpu = Apply(_cuda, cpuTensor, a => a.AddPadding(2));
            using var mkl = Apply(_mkl, cpuTensor, a => a.AddPadding(2));
            AssertSame(cpu, gpu, mkl);
        }

        void CheckTensorIm2Col(uint rows, uint columns, uint depth, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool randomData)
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var cpuTensor = _cpu.CreateTensor3D(depth.AsRange().Select(i => _cpu.CreateMatrix(rows, columns, (j, k) => randomData
                ? Convert.ToSingle(normalDistribution.Sample())
                : Convert.ToSingle((i + 1) * (j + 1) * (k + 1))
            )).ToArray());
            using var cpu = cpuTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
            using var gpu = Apply(_cuda, cpuTensor, a => a.Im2Col(filterWidth, filterHeight, xStride, yStride));
            using var mkl = Apply(_mkl, cpuTensor, a => a.Im2Col(filterWidth, filterHeight, xStride, yStride));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorIm2Col()
        {
            CheckTensorIm2Col(4, 4, 3, 2, 2, 2, 2, false);
        }

        [Fact]
        public void TensorIm2Col2()
        {
            CheckTensorIm2Col(8, 8, 1, 2, 2, 2, 2, true);
        }

        [Fact]
        public void TensorIm2Col3()
        {
            CheckTensorIm2Col(8, 8, 2, 2, 2, 1, 1, true);
        }

        [Fact]
        public void TensorIm2Col4()
        {
            CheckTensorIm2Col(8, 8, 2, 1, 2, 1, 1, true);
        }

        [Fact]
        public void TensorIm2Col5()
        {
            CheckTensorIm2Col(8, 8, 2, 2, 1, 1, 1, true);
        }

        [Fact]
        public void TensorIm2Col6()
        {
            CheckTensorIm2Col(8, 8, 3, 2, 1, 2, 2, true);
        }

        [Fact]
        public void TensorIm2Col7()
        {
            CheckTensorIm2Col(8, 8, 3, 2, 1, 1, 1, true);
        }

        [Fact]
        public void TensorIm2Col8()
        {
            CheckTensorIm2Col(8, 8, 3, 8, 1, 1, 1, true);
        }

        [Fact]
        public void TensorIm2Col9()
        {
            CheckTensorIm2Col(12, 8, 1, 4, 1, 1, 1, true);
        }

        static void AssertValuesAreInSamePlace(ITensor3D maxPool, ITensor3D source)
        {
            for (uint z = 0; z < maxPool.Depth; z++) {
                var slice = maxPool.GetMatrix(z);
                for (uint i = 0; i < slice.RowCount; i++) {
                    for (uint j = 0; j < slice.ColumnCount; j++) {
                        var val = slice[i, j];
                        if (FloatMath.IsNotZero(val)) {
                            val.Should().Be(source[z, i, j]);
                        }
                    }
                }
            }
        }

        [Fact]
        public void TensorMaxPool()
        {
            const uint filterWidth = 2, filterHeight = 2, xStride = 2, yStride = 2, inputWidth = 4, inputHeight = 4;
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, 2).Select(_ => _cpu.CreateMatrix(inputHeight, inputWidth, true)).ToArray());
            cpuTensor[0, 0, 0] = 1f;
            cpuTensor[0, 3, 0] = 2f;
            cpuTensor[0, 0, 3] = 3f;
            cpuTensor[0, 3, 3] = 4f;

            cpuTensor[1, 1, 1] = 1f;
            cpuTensor[1, 2, 1] = 2f;
            cpuTensor[1, 1, 2] = 3f;
            cpuTensor[1, 2, 2] = 4f;

            var (cpuMaxPool, cpuIndex) = cpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            using var cpu = cpuMaxPool.ReverseMaxPool(cpuIndex!, inputHeight, inputWidth, filterWidth, filterHeight, xStride, yStride);
            
            using var gpuTensor = _cuda.CreateTensor3D(cpuTensor);
            var (gpuMaxPool, gpuIndex) = gpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            using var gpu = gpuMaxPool.ReverseMaxPool(gpuIndex!, inputHeight, inputWidth, filterWidth, filterHeight, xStride, yStride);

            using var mklTensor = _mkl.CreateTensor3D(cpuTensor);
            var (mklMaxPool, mklIndex) = mklTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            using var mkl = mklMaxPool.ReverseMaxPool(mklIndex!, inputHeight, inputWidth, filterWidth, filterHeight, xStride, yStride);
            try {
                AssertSame(cpuTensor, cpu);
                AssertSame(cpuMaxPool, gpuMaxPool, mklMaxPool);
                AssertSame(cpu, gpu, mkl);
            }
            finally {
                cpuMaxPool.Dispose();
                cpuIndex!.Dispose();
                gpuMaxPool.Dispose();
                gpuIndex!.Dispose();
                mklMaxPool.Dispose();
                mklIndex!.Dispose();
            }
        }

        [Fact]
        public void TensorMaxPool2()
        {
            CheckTensorMaxPool(8, 8, 3, 2, 2, 2, 2, false, true);
        }

        [Fact]
        public void TensorMaxPool3()
        {
            CheckTensorMaxPool(8, 8, 3, 2, 2, 2, 2, true, false);
        }

        [Fact]
        public void TensorMaxPool4()
        {
            CheckTensorMaxPool(8, 8, 3, 2, 2, 2, 2, true, true);
        }

        [Fact]
        public void TensorMaxPool5()
        {
            CheckTensorMaxPool(8, 8, 3, 2, 1, 2, 2, true, true);
        }

        [Fact]
        public void TensorMaxPool6()
        {
            CheckTensorMaxPool(8, 8, 3, 1, 2, 2, 2, true, true);
        }

        [Fact]
        public void TensorMaxPool7()
        {
            CheckTensorMaxPool(8, 8, 2, 2, 1, 1, 1, true, true);
        }

        [Fact]
        public void TensorMaxPool8()
        {
            CheckTensorMaxPool(8, 8, 1, 1, 2, 1, 1, true, true);
        }

        [Fact]
        public void TensorMaxPoolBlankIrregular()
        {
            const int rows = 7, columns = 7, depth = 8, filterWidth = 2, filterHeight = 2, xStride = 2, yStride = 2;
            using var cpuTensor = _cpu.CreateTensor3D(Enumerable.Range(0, depth).Select(_ => _cpu.CreateMatrix(rows, columns, (_, _) => 0)).ToArray());

            var (cpuMaxPool, cpuIndices) = cpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(cpuIndices!, rows, columns, filterWidth, filterHeight, xStride, yStride);
            AssertValuesAreInSamePlace(cpuReverseMaxPool, cpuTensor);

            using var gpuTensor = _cuda.CreateTensor3D(cpuTensor);
            var (gpuMaxPool, gpuIndices) = gpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            FloatMath.AreApproximatelyEqual(gpuMaxPool, cpuMaxPool);
            FloatMath.AreApproximatelyEqual(gpuIndices!, cpuIndices!);
            using var gpuReverseMaxPool = gpuMaxPool.ReverseMaxPool(gpuIndices!, rows, columns, filterWidth, filterHeight, xStride, yStride);

            using var mklTensor = _mkl.CreateTensor3D(cpuTensor);
            var (mklMaxPool, mklIndices) = mklTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            FloatMath.AreApproximatelyEqual(mklMaxPool, cpuMaxPool);
            FloatMath.AreApproximatelyEqual(mklIndices!, cpuIndices!);
            using var mklReverseMaxPool = mklMaxPool.ReverseMaxPool(mklIndices!, rows, columns, filterWidth, filterHeight, xStride, yStride);

            try {
                AssertSame(cpuReverseMaxPool, gpuReverseMaxPool, mklReverseMaxPool);
            }
            finally {
                cpuMaxPool.Dispose();
                cpuIndices!.Dispose();
                gpuMaxPool.Dispose();
                gpuIndices!.Dispose();
                mklMaxPool.Dispose();
                mklIndices!.Dispose();
            }
        }

        void CheckTensorMaxPool(uint rows, uint columns, uint depth, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool randomInit, bool calculateIndices)
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var cpuTensor = _cpu.CreateTensor3D(depth.AsRange().Select(i => _cpu.CreateMatrix(rows, columns, (j, k) => randomInit
                ? Convert.ToSingle(normalDistribution.Sample())
                : Convert.ToSingle((i + 1) * (j + 1) * (k + 1))
            )).ToArray());

            var (cpuMaxPool, cpuIndices) = cpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            using var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(cpuIndices!, rows, columns, filterWidth, filterHeight, xStride, yStride);
            AssertValuesAreInSamePlace(cpuReverseMaxPool, cpuTensor);

            using var gpuTensor = _cuda.CreateTensor3D(cpuTensor);
            var (gpuMaxPool, gpuIndices) = gpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, calculateIndices);
            if (calculateIndices) {
                FloatMath.AreApproximatelyEqual(gpuIndices!, cpuIndices!);
                using var gpuReverseMaxPool = gpuMaxPool.ReverseMaxPool(gpuIndices!, rows, columns, filterWidth, filterHeight, xStride, yStride);
                FloatMath.AreApproximatelyEqual(gpuReverseMaxPool, cpuReverseMaxPool);
            }

            using var mklTensor = _mkl.CreateTensor3D(cpuTensor);
            var (mklMaxPool, mklIndices) = mklTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, calculateIndices);
            if (calculateIndices) {
                FloatMath.AreApproximatelyEqual(mklIndices!, cpuIndices!);
                using var mklReverseMaxPool = mklMaxPool.ReverseMaxPool(mklIndices!, rows, columns, filterWidth, filterHeight, xStride, yStride);
                FloatMath.AreApproximatelyEqual(mklReverseMaxPool, cpuReverseMaxPool);
            }

            try {
                AssertSame(cpuMaxPool, gpuMaxPool, mklMaxPool);
            }
            finally {
                cpuMaxPool.Dispose();
                cpuIndices?.Dispose();
                gpuMaxPool.Dispose();
                gpuIndices?.Dispose();
                mklMaxPool.Dispose();
                mklIndices?.Dispose();
            }
        }

        void CheckTensorReverseIm2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride, uint depth, uint filterCount, uint inputWidth, uint inputHeight)
        {
            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            using var cpuTensor = _cpu.CreateTensor3D(depth.AsRange().Select(_ => _cpu.CreateMatrix(inputHeight, inputWidth, (_, _) => Convert.ToSingle(normalDistribution.Sample()))).ToArray());
            using var im2Col = cpuTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
            using var cpuFilter = _cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (_, _) => normalDistribution.Sample());
            using var output = im2Col.Multiply(cpuFilter);

            var matrixList = new IMatrix[output.ColumnCount];
            var newWidth = ((inputWidth - filterWidth) / xStride) + 1;
            var newHeight = ((inputHeight - filterHeight) / yStride) + 1;
            for (uint i = 0; i < output.ColumnCount; i++)
                matrixList[i] = output.GetColumnVector(i).Reshape(newWidth, newHeight);
            var outputTensor = _cpu.CreateTensor3DAndThenDisposeInput(matrixList);

            using var gpuTensor = _cuda.CreateTensor3D(outputTensor);
            FloatMath.AreApproximatelyEqual(gpuTensor, outputTensor);
            using var gpuFilter = _cuda.CreateMatrix(cpuFilter);

            using var mklTensor = _mkl.CreateTensor3D(outputTensor);
            FloatMath.AreApproximatelyEqual(mklTensor, outputTensor);
            using var mklFilter = _mkl.CreateMatrix(cpuFilter);

            using var cpuReverseIm2Col = outputTensor.ReverseIm2Col(cpuFilter, inputHeight, inputWidth, depth, filterWidth, filterHeight, xStride, yStride);
            using var gpuReverseIm2Col = gpuTensor.ReverseIm2Col(gpuFilter, inputHeight, inputWidth, depth, filterWidth, filterHeight, xStride, yStride);
            FloatMath.AreApproximatelyEqual(gpuReverseIm2Col, cpuReverseIm2Col);

            using var mklReverseIm2Col = mklTensor.ReverseIm2Col(mklFilter, inputHeight, inputWidth, depth, filterWidth, filterHeight, xStride, yStride);
            FloatMath.AreApproximatelyEqual(mklReverseIm2Col, cpuReverseIm2Col);
        }

        [Fact]
        public void TensorReverseIm2Col()
        {
            CheckTensorReverseIm2Col(2, 2, 2, 2, 1, 1, 4, 4);
        }

        [Fact]
        public void TensorReverseIm2Col2()
        {
            CheckTensorReverseIm2Col(2, 2, 2, 2, 1, 2, 4, 4);
        }

        [Fact]
        public void TensorReverseIm2Col3()
        {
            CheckTensorReverseIm2Col(2, 2, 2, 2, 2, 1, 4, 4);
        }

        [Fact]
        public void TensorReverseIm2Col4()
        {
            CheckTensorReverseIm2Col(2, 2, 2, 2, 2, 2, 4, 4);
        }

        [Fact]
        public void TensorReverseIm2Col5()
        {
            CheckTensorReverseIm2Col(2, 2, 1, 1, 2, 2, 4, 4);
        }

        [Fact]
        public void TensorReverseIm2Col6()
        {
            CheckTensorReverseIm2Col(2, 1, 1, 1, 1, 1, 4, 4);
        }

        [Fact]
        public void TensorReverseIm2Col7()
        {
            CheckTensorReverseIm2Col(10, 3, 1, 1, 1, 2, 10, 12);
        }

        IReadOnlyMatrix CheckCreateMatrix(uint depth, uint rows, uint columns, Func<uint, uint, uint, float>? valueProvider)
        {
            return _context.CreateReadOnlyMatrixFromRows(rows.AsRange().Select(i => _context.CreateReadOnlyVector(columns, j => valueProvider?.Invoke(i, j, depth) ?? 0f)).ToArray());
        }

        IReadOnlyTensor3D CheckCreateTensor(uint rows, uint columns, uint depth, Func<uint, uint, uint, float>? valueProvider)
        {
            return _context.CreateReadOnlyTensor3D(depth.AsRange().Select(k => CheckCreateMatrix(k, rows, columns, valueProvider)).ToArray());
        }

        [Fact]
        public void Tensor4DCreate()
        {
            const uint rows = 3, columns = 4, depth = 2, count = 5;
            var data = count.AsRange().Select(_ => CheckCreateTensor(rows, columns, depth, (i, j, d) => (j + 1) * (i + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var gpuTensor = _cuda.CreateTensor4D(data);
            FloatMath.AreApproximatelyEqual(cpuTensor, gpuTensor);

            using var mklTensor = _mkl.CreateTensor4D(data);
            FloatMath.AreApproximatelyEqual(cpuTensor, mklTensor);
        }

        [Fact]
        public void Tensor4DAddPadding()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpuResult = cpuTensor.AddPadding(1);

            using var gpuResult = Apply(_cuda, cpuTensor, a => a.AddPadding(1));
            FloatMath.AreApproximatelyEqual(gpuResult, cpuResult);

            var mklResult = Apply(_mkl, cpuTensor, a => a.AddPadding(1));
            FloatMath.AreApproximatelyEqual(mklResult, cpuResult);
        }

        [Fact]
        public void Tensor4DRemovePadding()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpuResult = cpuTensor.RemovePadding(1);

            using var gpuResult = Apply(_cuda, cpuTensor, a => a.RemovePadding(1));
            FloatMath.AreApproximatelyEqual(gpuResult, cpuResult);

            using var mklResult = Apply(_mkl, cpuTensor, a => a.RemovePadding(1));
            FloatMath.AreApproximatelyEqual(mklResult, cpuResult);
        }

        [Fact]
        public void TensorCombineDepthSlices()
        {
            var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var cpuTensor = _cpu.CreateTensor3D(tensor);
            using var cpuResult = cpuTensor.AddAllMatrices();

            using var gpuResult = Apply(_cuda, cpuTensor, a => a.AddAllMatrices());
            FloatMath.AreApproximatelyEqual(gpuResult, cpuResult);

            using var mklResult = Apply(_mkl, cpuTensor, a => a.AddAllMatrices());
            FloatMath.AreApproximatelyEqual(mklResult, cpuResult);
        }

        [Fact]
        public void TensorCombineDepthSlices2()
        {
            var tensor = CheckCreateTensor(12, 6, 3, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var cpuTensor = _cpu.CreateTensor3D(tensor);
            using var cpuResult = cpuTensor.AddAllMatrices();

            using var gpuResult = Apply(_cuda, cpuTensor, a => a.AddAllMatrices());
            FloatMath.AreApproximatelyEqual(gpuResult, cpuResult);

            using var mklResult = Apply(_mkl, cpuTensor, a => a.AddAllMatrices());
            FloatMath.AreApproximatelyEqual(mklResult, cpuResult);
        }

        [Fact]
        public void TensorAddInPlace()
        {
            var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            var tensor2 = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var cpuTensor = _cpu.CreateTensor3D(tensor);
            using var cpuTensor2 = _cpu.CreateTensor3D(tensor2);
            cpuTensor.AddInPlace(cpuTensor2);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            using var gpuTensor2 = _cuda.CreateTensor3D(tensor2);
            gpuTensor.AddInPlace(gpuTensor2);
            FloatMath.AreApproximatelyEqual(cpuTensor, gpuTensor);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            using var mklTensor2 = _mkl.CreateTensor3D(tensor2);
            mklTensor.AddInPlace(mklTensor2);
            FloatMath.AreApproximatelyEqual(cpuTensor, mklTensor);
        }

        [Fact]
        public void Tensor4DMaxPool()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpuPooled = cpuTensor.MaxPool(2, 2, 2, 2, false).Result;

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpuPooled = gpuTensor.MaxPool(2, 2, 2, 2, false).Result;
            FloatMath.AreApproximatelyEqual(cpuPooled, gpuPooled);

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mklPooled = mklTensor.MaxPool(2, 2, 2, 2, false).Result;
            FloatMath.AreApproximatelyEqual(cpuPooled, mklPooled);
        }

        [Fact]
        public void Tensor4DReverseMaxPool()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

            using var cpuTensor = _cpu.CreateTensor4D(data);
            var (cpuResult, cpuIndices) = cpuTensor.MaxPool(2, 2, 2, 2, true);
            using var cpu = cpuResult.ReverseMaxPool(cpuIndices!, 4, 4, 2, 2, 2, 2);

            using var gpuTensor = _cuda.CreateTensor4D(data);
            var (gpuResult, gpuIndices) = gpuTensor.MaxPool(2, 2, 2, 2, true);
            using var gpu = gpuResult.ReverseMaxPool(gpuIndices!, 4, 4, 2, 2, 2, 2);

            using var mklTensor = _mkl.CreateTensor4D(data);
            var (mklResult, mklIndices) = mklTensor.MaxPool(2, 2, 2, 2, true);
            using var mkl = mklResult.ReverseMaxPool(mklIndices!, 4, 4, 2, 2, 2, 2);
            try {
                AssertSame(cpu, gpu, mkl);
            }
            finally {
                cpuResult.Dispose();
                cpuIndices!.Dispose();
                gpuResult.Dispose();
                gpuIndices!.Dispose();
                mklResult.Dispose();
                mklIndices!.Dispose();
            }
        }

        [Fact]
        public void Tensor4DIm2Col()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1) * (z + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpu = cpuTensor.Im2Col(2, 2, 1, 1);

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpu = gpuTensor.Im2Col(2, 2, 1, 1);

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mkl = mklTensor.Im2Col(2, 2, 1, 1);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorMultiplyMatrix()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpuIm2Col = cpuTensor.Im2Col(2, 2, 1, 1);
            using var cpuFilter = _cpu.CreateMatrix(2 * 2 * 2, 5, (i, j) => (i + 1) * (j + 1));
            using var cpu = cpuIm2Col.MultiplyEachMatrixBy(cpuFilter);

            using var gpuIm2Col = _cuda.CreateTensor3D(cpuIm2Col);
            using var gpuFilter = _cuda.CreateMatrix(cpuFilter);
            using var gpu = gpuIm2Col.MultiplyEachMatrixBy(gpuFilter);

            using var mklIm2Col = _mkl.CreateTensor3D(cpuIm2Col);
            using var mklFilter = _mkl.CreateMatrix(cpuFilter);
            using var mkl = mklIm2Col.MultiplyEachMatrixBy(mklFilter);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void TensorTransposeMultiplyMatrix()
        {
            var tensor = CheckCreateTensor(4, 4, 4, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var cpuTensor = _cpu.CreateTensor3D(tensor);
            using var cpuFilter = _cpu.CreateMatrix(4, 4, (i, j) => (i + 1) * (j + 1));
            using var cpu = cpuTensor.TransposeAndMultiplyEachMatrixBy(cpuFilter);

            using var gpuIm2Col = _cuda.CreateTensor3D(tensor);
            using var gpuFilter = _cuda.CreateMatrix(cpuFilter);
            using var gpu = gpuIm2Col.TransposeAndMultiplyEachMatrixBy(gpuFilter);

            using var mklIm2Col = _mkl.CreateTensor3D(tensor);
            using var mklFilter = _mkl.CreateMatrix(cpuFilter);
            using var mkl = mklIm2Col.TransposeAndMultiplyEachMatrixBy(mklFilter);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void Tensor3DToVector()
        {
            var data = CheckCreateTensor(4, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var tensor = _cpu.CreateTensor3D(data);
            using var vector = _cpu.CreateVector(data.Segment);
            using var tensor2 = vector.Reshape(4, 3, 2);
            FloatMath.AreApproximatelyEqual(tensor, tensor2);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            using var gpuVector = _cuda.CreateVector(data.Segment);
            using var gpuTensor2 = gpuVector.Reshape(4, 3, 2);
            FloatMath.AreApproximatelyEqual(gpuTensor, gpuTensor2);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            using var mklVector = _mkl.CreateVector(data.Segment);
            using var mklTensor2 = mklVector.Reshape(4, 3, 2);
            FloatMath.AreApproximatelyEqual(mklTensor, mklTensor2);
        }

        [Fact]
        public void Tensor3DToVector2()
        {
            var data = CheckCreateTensor(4, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var tensor = _cpu.CreateTensor3D(data);
            using var vector = tensor.Reshape();
            using var tensor2 = vector.Reshape(4, 3, 2);
            FloatMath.AreApproximatelyEqual(tensor, tensor2);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            using var gpuVector = gpuTensor.Reshape();
            using var gpuTensor2 = gpuVector.Reshape(4, 3, 2);
            FloatMath.AreApproximatelyEqual(gpuTensor, gpuTensor2);
            FloatMath.AreApproximatelyEqual(vector, gpuVector);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            using var mklVector = mklTensor.Reshape();
            using var mklTensor2 = mklVector.Reshape(4, 3, 2);
            FloatMath.AreApproximatelyEqual(mklTensor, mklTensor2);
            FloatMath.AreApproximatelyEqual(vector, mklVector);
        }

        [Fact]
        public void Tensor3DToMatrix()
        {
            var data = CheckCreateTensor(4, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var tensor = _cpu.CreateTensor3D(data);
            using var matrix = tensor.ReshapeAsMatrix();
            using var tensor2 = matrix.Reshape(null, 4, 3);
            FloatMath.AreApproximatelyEqual(tensor, tensor2);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            using var gpuMatrix = gpuTensor.ReshapeAsMatrix();
            using var gpuTensor2 = gpuMatrix.Reshape(null, 4, 3);
            FloatMath.AreApproximatelyEqual(gpuTensor, gpuTensor2);
            FloatMath.AreApproximatelyEqual(matrix, gpuMatrix);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            using var mklMatrix = mklTensor.ReshapeAsMatrix();
            using var mklTensor2 = mklMatrix.Reshape(null, 4, 3);
            FloatMath.AreApproximatelyEqual(mklTensor, mklTensor2);
            FloatMath.AreApproximatelyEqual(matrix, mklMatrix);
        }

        [Fact]
        public void Tensor3DToTensor4D()
        {
            var data = CheckCreateTensor(12, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var tensor = _cpu.CreateTensor3D(data);
            using var tensor2 = tensor.Reshape(3, 4, 3, 2);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            using var gpuTensor2 = gpuTensor.Reshape(3, 4, 3, 2);
            FloatMath.AreApproximatelyEqual(tensor2, gpuTensor2);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            using var mklTensor2 = mklTensor.Reshape(3, 4, 3, 2);
            FloatMath.AreApproximatelyEqual(tensor2, mklTensor2);
        }

        [Fact]
        public void Tensor4DToMatrix()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1) * (z + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpuMatrix = cpuTensor.ReshapeAsMatrix();
            using var cpuTensor2 = cpuMatrix.Reshape(null, 3, 4, 2);
            FloatMath.AreApproximatelyEqual(cpuTensor, cpuTensor2);

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpuMatrix = gpuTensor.ReshapeAsMatrix();
            using var gpuTensor2 = gpuMatrix.Reshape(null, 3, 4, 2);
            FloatMath.AreApproximatelyEqual(gpuTensor, gpuTensor2);
            FloatMath.AreApproximatelyEqual(cpuMatrix, gpuMatrix);

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mklMatrix = mklTensor.ReshapeAsMatrix();
            using var mklTensor2 = mklMatrix.Reshape(null, 3, 4, 2);
            FloatMath.AreApproximatelyEqual(mklTensor, mklTensor2);
            FloatMath.AreApproximatelyEqual(cpuMatrix, mklMatrix);
        }

        [Fact]
        public void Tensor3DAddVectorToEachRow()
        {
            var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var cpuTensor = _cpu.CreateTensor3D(tensor);
            using var cpuVector = _cpu.CreateVector(4, 1f);
            cpuTensor.AddToEachRow(cpuVector);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            using var gpuVector = _cuda.CreateVector(4, 1f);
            gpuTensor.AddToEachRow(gpuVector);
            FloatMath.AreApproximatelyEqual(cpuTensor, gpuTensor);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            using var mklVector = _mkl.CreateVector(4, 1f);
            mklTensor.AddToEachRow(mklVector);
            FloatMath.AreApproximatelyEqual(cpuTensor, mklTensor);
        }

        [Fact]
        public void Tensor3DAddVectorToEachColumn()
        {
            var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var cpuTensor = _cpu.CreateTensor3D(tensor);
            using var cpuVector = _cpu.CreateVector(2, 1f);
            cpuTensor.AddToEachColumn(cpuVector);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            using var gpuVector = _cuda.CreateVector(4, 1f);
            gpuTensor.AddToEachColumn(gpuVector);
            FloatMath.AreApproximatelyEqual(cpuTensor, gpuTensor);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            using var mklVector = _mkl.CreateVector(4, 1f);
            mklTensor.AddToEachColumn(mklVector);
            FloatMath.AreApproximatelyEqual(cpuTensor, mklTensor);
        }

        [Fact]
        public void Tensor3DToFloatTensor()
        {
            var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
            using var cpuTensor = _cpu.CreateTensor3D(tensor);
            FloatMath.AreApproximatelyEqual(cpuTensor, tensor);

            using var gpuTensor = _cuda.CreateTensor3D(tensor);
            FloatMath.AreApproximatelyEqual(cpuTensor, gpuTensor);

            using var mklTensor = _mkl.CreateTensor3D(tensor);
            FloatMath.AreApproximatelyEqual(cpuTensor, mklTensor);
        }

        [Fact]
        public void Tensor3DTransposeThisAndMultiply()
        {
            var normalDistribution = _context.CreateNormalDistribution();
            var tensor1 = CheckCreateTensor(9, 3, 3, (_, _, _) => normalDistribution.Sample());
            var data = 3.AsRange().Select(_ => CheckCreateTensor(3, 3, 3, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

            using var cpuTensor1 = _cpu.CreateTensor3D(tensor1);
            using var cpuTensor2 = _cpu.CreateTensor4D(data);
            using var cpu = cpuTensor1.TransposeThisAndMultiply(cpuTensor2);

            using var gpuTensor1 = _cuda.CreateTensor3D(tensor1);
            using var gpuTensor2 = _cuda.CreateTensor4D(data);
            var gpu = gpuTensor1.TransposeThisAndMultiply(gpuTensor2);

            using var mklTensor1 = _mkl.CreateTensor3D(tensor1);
            using var mklTensor2 = _mkl.CreateTensor4D(data);
            var mkl = mklTensor1.TransposeThisAndMultiply(mklTensor2);
            AssertSameWithMaxDifference(8, cpu, gpu, mkl);
        }

        [Fact]
        public void Tensor3DMultiply()
        {
            var normalDistribution = _context.CreateNormalDistribution();
            var tensor1 = CheckCreateTensor(3, 9, 3, (_, _, _) => normalDistribution.Sample());
            var data = 3.AsRange().Select(_ => CheckCreateTensor(3, 3, 3, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

            using var cpuTensor1 = _cpu.CreateTensor3D(tensor1);
            using var cpuTensor2 = _cpu.CreateTensor4D(data);
            using var cpu = cpuTensor1.Multiply(cpuTensor2);

            using var gpuTensor1 = _cuda.CreateTensor3D(tensor1);
            using var gpuTensor2 = _cuda.CreateTensor4D(data);
            var gpu = gpuTensor1.Multiply(gpuTensor2);

            using var mklTensor1 = _mkl.CreateTensor3D(tensor1);
            using var mklTensor2 = _mkl.CreateTensor4D(data);
            var mkl = mklTensor1.Multiply(mklTensor2);
            AssertSameWithMaxDifference(8, cpu, gpu, mkl);
        }

        [Fact]
        public void Tensor4DCounts()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);

            cpuTensor.Count.Should().Be(5);
            cpuTensor.Depth.Should().Be(2);
            cpuTensor.ColumnCount.Should().Be(4);
            cpuTensor.RowCount.Should().Be(3);

            using var gpuTensor = _cuda.CreateTensor4D(data);
            cpuTensor.Count.Should().Be(gpuTensor.Count);
            cpuTensor.Depth.Should().Be(gpuTensor.Depth);
            cpuTensor.ColumnCount.Should().Be(gpuTensor.ColumnCount);
            cpuTensor.RowCount.Should().Be(gpuTensor.RowCount);

            using var mklTensor = _mkl.CreateTensor4D(data);
            cpuTensor.Count.Should().Be(mklTensor.Count);
            cpuTensor.Depth.Should().Be(mklTensor.Depth);
            cpuTensor.ColumnCount.Should().Be(mklTensor.ColumnCount);
            cpuTensor.RowCount.Should().Be(mklTensor.RowCount);
        }

        [Fact]
        public void Tensor4DColumnSums()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpu = cpuTensor.ColumnSums();

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpu = gpuTensor.ColumnSums();

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mkl = mklTensor.ColumnSums();
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void Tensor4DRowSums()
        {
            var data = Enumerable.Range(0, 5)
                .Select(_ => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpu = cpuTensor.RowSums();

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpu = gpuTensor.RowSums();

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mkl = mklTensor.RowSums();
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void Tensor4DGetTensorAt()
        {
            var data = 5.AsRange()
                .Select(_ => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
            using var cpu4dTensor = _cpu.CreateTensor4D(data);

            using var gpu4dTensor = _cuda.CreateTensor4D(data);
            for (uint i = 0; i < 5; i++) {
                using var cpuTensor = cpu4dTensor.GetTensor(i);
                using var gpuTensor = gpu4dTensor.GetTensor(i);
                AssertSame(cpuTensor, gpuTensor);
            }

            using var mkl4dTensor = _mkl.CreateTensor4D(data);
            for (uint i = 0; i < 5; i++) {
                using var cpuTensor = cpu4dTensor.GetTensor(i);
                using var mklTensor = mkl4dTensor.GetTensor(i);
                AssertSame(cpuTensor, mklTensor);
            }
        }

        [Fact]
        public void Tensor4DReverseIm2Col()
        {
            const int rows = 4, columns = 4, depth = 1, count = 1, filterWidth = 2, filterHeight = 2, filterCount = 1, xStride = 2, yStride = 2;

            var normalDistribution = _context.CreateNormalDistribution(0, 1);
            var data = Enumerable.Range(0, count)
                .Select(_ => CheckCreateTensor(rows, columns, depth, (_, _, _) => normalDistribution.Sample())).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpuFilter = _cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (_, _) => normalDistribution.Sample());

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpuFilter = _cuda.CreateMatrix(cpuFilter);
            AssertSame(cpuTensor, gpuTensor);

            var cpuReverseIm2Col = cpuTensor.ReverseIm2Col(cpuFilter, rows, columns, depth, filterWidth, filterHeight, xStride, yStride);
            using var gpuReverseIm2Col = gpuTensor.ReverseIm2Col(gpuFilter, rows, columns, depth, filterWidth, filterHeight, xStride, yStride);

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mklFilter = _mkl.CreateMatrix(cpuFilter);
            AssertSame(cpuTensor, mklTensor);

            using var mklReverseIm2Col = mklTensor.ReverseIm2Col(mklFilter, rows, columns, depth, filterWidth, filterHeight, xStride, yStride);
            AssertSame(cpuReverseIm2Col, gpuReverseIm2Col, mklReverseIm2Col);
        }

        [Fact]
        public void Tensor4DReshapeAsVector()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1) * (z + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpu = cpuTensor.Reshape();

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpu = gpuTensor.Reshape();

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mkl = mklTensor.Reshape();
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void Tensor4DReshapeAsMatrix()
        {
            var data = Enumerable.Range(0, 5)
                .Select(z => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1) * (z + 1))).ToArray();
            using var cpuTensor = _cpu.CreateTensor4D(data);
            using var cpu = cpuTensor.Reshape();

            using var gpuTensor = _cuda.CreateTensor4D(data);
            using var gpu = gpuTensor.Reshape();

            using var mklTensor = _mkl.CreateTensor4D(data);
            using var mkl = mklTensor.Reshape();
            AssertSame(cpu, gpu, mkl);
        }
    }
}
