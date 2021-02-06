using System;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using FluentAssertions;
using MathNet.Numerics.Distributions;
using Xunit;

namespace BrightData.UnitTests
{
    public class TensorTests : CudaBase
    {
		[Fact]
		public void TensorConvertToVector()
        {
            using var cpuTensor = _cpu.Create3DTensor(3.AsRange().Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            using var cpuVector = cpuTensor.ReshapeAsVector();
            using var gpuVector = gpuTensor.ReshapeAsVector();
            FloatMath.AreApproximatelyEqual(cpuVector.AsIndexable(), gpuVector.AsIndexable());
        }

		[Fact]
		public void TensorCreateFromVector()
		{
			const int DEPTH = 3, ROWS = 4, COLUMNS = 4;
			var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(ROWS, COLUMNS, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
			var cpuVector = cpuTensor.ReshapeAsVector();
			var cpuTensor2 = cpuVector.ReshapeAs3DTensor(ROWS, COLUMNS, DEPTH);
			FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), cpuTensor2.AsIndexable());

            using var gpuVector = _cuda.CreateVector(cpuVector.AsIndexable());
            using var gpuTensor2 = gpuVector.ReshapeAs3DTensor(ROWS, COLUMNS, DEPTH);
            FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
        }

		[Fact]
		public void TensorConvertToMatrix()
        {
            using var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            using var cpuMatrix = cpuTensor.ReshapeAsMatrix();
            using var gpuMatrix = gpuTensor.ReshapeAsMatrix();
            FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable());
        }

		[Fact]
		public void TensorCreateFromMatrix()
		{
			const int DEPTH = 3, ROWS = 4, COLUMNS = 4;
			var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, DEPTH).Select(i => _cpu.CreateMatrix(ROWS, COLUMNS, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
			var cpuMatrix = cpuTensor.ReshapeAsMatrix();
			var cpuTensor2 = cpuMatrix.ReshapeAs3DTensor(ROWS, COLUMNS);
			FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), cpuTensor2.AsIndexable());

            using var gpuMatrix = _cuda.CreateMatrix(cpuMatrix.AsIndexable());
            using var gpuTensor2 = gpuMatrix.ReshapeAs3DTensor(ROWS, COLUMNS);
            FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
        }

		[Fact]
		public void TensorAddPadding()
        {
            using var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            using var cpuPadding = cpuTensor.AddPadding(1);
            using var gpuPadding = gpuTensor.AddPadding(1);
            FloatMath.AreApproximatelyEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
        }

		[Fact]
		public void TensorRemovePadding()
        {
            using var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            using var cpuPadding = cpuTensor.RemovePadding(1);
            using var gpuPadding = gpuTensor.RemovePadding(1);
            FloatMath.AreApproximatelyEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
        }

		[Fact]
		public void TensorAddPadding2()
        {
            using var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 3).Select(i => _cpu.CreateMatrix(4, 4, (j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray());
            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            using var cpuPadding = cpuTensor.AddPadding(2);
            using var gpuPadding = gpuTensor.AddPadding(2);
            FloatMath.AreApproximatelyEqual(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());
        }

		void CheckTensorIm2Col(uint rows, uint columns, uint depth, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool randomData)
		{
			var normalDistribution = new Normal(0, 1);
            using var cpuTensor = _cpu.Create3DTensor(depth.AsRange().Select(i => _cpu.CreateMatrix(rows, columns, (j, k) => randomData
                ? Convert.ToSingle(normalDistribution.Sample())
                : Convert.ToSingle((i + 1) * (j + 1) * (k + 1))
            )).ToArray());
            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            using var cpuMatrix = cpuTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
            using var gpuMatrix = gpuTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
            var cpu = cpuMatrix.AsIndexable();
            var gpu = gpuMatrix.AsIndexable();
            FloatMath.AreApproximatelyEqual(cpu, gpu);
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

		//void _AssertAreSame(IReadOnlyList<(uint[] X, uint[] Y)> cpuIndex, IReadOnlyList<(uint[] X, uint[] Y)> gpuIndex)
  //      {
  //          cpuIndex.Count.Should().Be(gpuIndex.Count);
  //          for (var i = 0; i < cpuIndex.Count; i++) {
		//		var list1 = cpuIndex[i];
		//		var list2 = gpuIndex[i];
  //              list1.Should().BeEquivalentTo(list2);
  //          }
		//}

		void AssertValuesAreInSamePlace(IIndexable3DFloatTensor maxPool, IIndexable3DFloatTensor source)
		{
			for (uint z = 0; z < maxPool.Depth; z++) {
				var slice = maxPool.GetMatrixAt(z).AsIndexable();
				for (uint i = 0; i < slice.RowCount; i++) {
					for (uint j = 0; j < slice.ColumnCount; j++) {
						var val = slice[i, j];
						if (FloatMath.IsNotZero(val)) {
                            val.Should().Be(source[i, j, z]);
                        }
					}
				}
			}
		}

		[Fact]
		public void TensorMaxPool()
		{
			const uint FILTER_WIDTH = 2, FILTER_HEIGHT = 2, XSTRIDE = 2, YSTRIDE = 2, INPUT_WIDTH = 4, INPUT_HEIGHT = 4;
			var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, 2).Select(i => _cpu.CreateZeroMatrix(INPUT_HEIGHT, INPUT_WIDTH)).ToArray()).AsIndexable();
			cpuTensor[0, 0, 0] = 1f;
			cpuTensor[0, 3, 0] = 2f;
			cpuTensor[3, 0, 0] = 3f;
			cpuTensor[3, 3, 0] = 4f;

			cpuTensor[1, 1, 1] = 1f;
			cpuTensor[1, 2, 1] = 2f;
			cpuTensor[2, 1, 1] = 3f;
			cpuTensor[2, 2, 1] = 4f;

			var (cpuMaxPool, cpuIndex) = cpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, XSTRIDE, YSTRIDE, true);
			var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(cpuIndex, INPUT_HEIGHT, INPUT_WIDTH, FILTER_WIDTH, FILTER_HEIGHT, XSTRIDE, YSTRIDE).AsIndexable();
			FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), cpuReverseMaxPool);

            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            var (gpuMaxPool, gpuIndex) = gpuTensor.MaxPool(FILTER_WIDTH, FILTER_HEIGHT, XSTRIDE, YSTRIDE, true);
            FloatMath.AreApproximatelyEqual(gpuMaxPool.AsIndexable(), cpuMaxPool.AsIndexable());
            using var gpuReverseMaxPool = gpuMaxPool.ReverseMaxPool(gpuIndex, INPUT_HEIGHT, INPUT_WIDTH, FILTER_WIDTH, FILTER_HEIGHT, XSTRIDE, YSTRIDE);
            FloatMath.AreApproximatelyEqual(gpuReverseMaxPool.AsIndexable(), cpuReverseMaxPool);
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
			var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, depth).Select(i => _cpu.CreateMatrix(rows, columns, (j, k) => 0)).ToArray()).AsIndexable();

			var (cpuMaxPool, cpuIndices) = cpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
			var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(cpuIndices, rows, columns, filterWidth, filterHeight, xStride, yStride).AsIndexable();
			AssertValuesAreInSamePlace(cpuReverseMaxPool, cpuTensor);

            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            var (gpuMaxPool, gpuIndices) = gpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
            FloatMath.AreApproximatelyEqual(gpuMaxPool.AsIndexable(), cpuMaxPool.AsIndexable());
            FloatMath.AreApproximatelyEqual(gpuIndices.AsIndexable(), cpuIndices.AsIndexable());
            using var gpuReverseMaxPool = gpuMaxPool.ReverseMaxPool(gpuIndices, rows, columns, filterWidth, filterHeight, xStride, yStride);
            FloatMath.AreApproximatelyEqual(gpuReverseMaxPool.AsIndexable(), cpuReverseMaxPool);
        }

		void CheckTensorMaxPool(uint rows, uint columns, uint depth, uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool randomInit, bool calculateIndices)
		{
			var normalDistribution = new Normal(0, 1);
			var cpuTensor = _cpu.Create3DTensor(depth.AsRange().Select(i => _cpu.CreateMatrix(rows, columns, (j, k) => randomInit
				? Convert.ToSingle(normalDistribution.Sample())
				: Convert.ToSingle((i + 1) * (j + 1) * (k + 1))
			)).ToArray()).AsIndexable();

			var (cpuMaxPool, cpuIndices) = cpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, true);
			var cpuReverseMaxPool = cpuMaxPool.ReverseMaxPool(cpuIndices, rows, columns, filterWidth, filterHeight, xStride, yStride).AsIndexable();
			AssertValuesAreInSamePlace(cpuReverseMaxPool, cpuTensor);

            using var gpuTensor = _cuda.Create3DTensor(cpuTensor.Data);
            var (gpuMaxPool, gpuIndices) = gpuTensor.MaxPool(filterWidth, filterHeight, xStride, yStride, calculateIndices);
            FloatMath.AreApproximatelyEqual(gpuMaxPool.AsIndexable(), cpuMaxPool.AsIndexable());
            if (calculateIndices) {
                FloatMath.AreApproximatelyEqual(gpuIndices.AsIndexable(), cpuIndices.AsIndexable());
                using var gpuReverseMaxPool = gpuMaxPool.ReverseMaxPool(gpuIndices, rows, columns, filterWidth, filterHeight, xStride, yStride);
                FloatMath.AreApproximatelyEqual(gpuReverseMaxPool.AsIndexable(), cpuReverseMaxPool);
            }
        }

		void CheckTensorReverseIm2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride, uint depth, uint filterCount, uint inputWidth, uint inputHeight)
		{
			var normalDistribution = new Normal(0, 1);
			var cpuTensor = _cpu.Create3DTensor(depth.AsRange().Select(i => _cpu.CreateMatrix(inputHeight, inputWidth, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToArray());
			var im2Col = cpuTensor.Im2Col(filterWidth, filterHeight, xStride, yStride);
			var cpuFilter = _cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (i, j) => (float)normalDistribution.Sample());
			var output = im2Col.Multiply(cpuFilter);

			var matrixList = new IFloatMatrix[output.ColumnCount];
			var newWidth = ((inputWidth - filterWidth) / xStride) + 1;
			var newHeight = ((inputHeight - filterHeight) / yStride) + 1;
			for (uint i = 0; i < output.ColumnCount; i++)
				matrixList[i] = output.Column(i).ReshapeAsMatrix(newWidth, newHeight);
			var outputTensor = _cpu.Create3DTensor(matrixList);

            using var gpuTensor = _cuda.Create3DTensor(outputTensor.Data);
            FloatMath.AreApproximatelyEqual(gpuTensor.AsIndexable(), outputTensor.AsIndexable());
            var gpuFilter = _cuda.CreateMatrix(cpuFilter.Data);

            var cpuReverseIm2Col = outputTensor.ReverseIm2Col(cpuFilter, inputHeight, inputWidth, depth, filterWidth, filterHeight, xStride, yStride);
            using var gpuReverseIm2Col = gpuTensor.ReverseIm2Col(gpuFilter, inputHeight, inputWidth, depth, filterWidth, filterHeight, xStride, yStride);
            FloatMath.AreApproximatelyEqual(gpuReverseIm2Col.AsIndexable(), cpuReverseIm2Col.AsIndexable());
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

        Matrix<float> CheckCreateMatrix(uint depth, uint rows, uint columns, Func<uint, uint, uint, float> valueProvider)
        {
            return _context.CreateMatrixFromRows(rows.AsRange().Select(i => _context.CreateVector(columns, j => valueProvider?.Invoke(i, j, depth) ?? 0f)).ToArray());
        }

		Tensor3D<float> CheckCreateTensor(uint rows, uint columns, uint depth, Func<uint, uint, uint, float> valueProvider)
        {
            return _context.CreateTensor3D(depth.AsRange().Select(k => CheckCreateMatrix(k, rows, columns, valueProvider)).ToArray());
        }

        [Fact]
		public void Tensor4DCreate()
		{
			const uint ROWS = 3, COLUMNS = 4, DEPTH = 2, COUNT = 5;
			var data = COUNT.AsRange().Select(z => CheckCreateTensor(ROWS, COLUMNS, DEPTH, null)).ToArray();
			for (var i = 0; i < COUNT; i++) {
				var item = data[i];
				for (uint j = 0; j < DEPTH; j++)
					item.Matrix(j).Row(1)[2] = (j + 1) * (i + 1);
			}
			var cpuTensor = _cpu.Create4DTensor(data).AsIndexable();
			var gpuTensor = _cuda.Create4DTensor(data).AsIndexable();
			FloatMath.AreApproximatelyEqual(cpuTensor, gpuTensor);
		}

		[Fact]
		public void Tensor4DAddPadding()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuPadded = cpuTensor.AddPadding(1);
            using var gpuTensor = _cuda.Create4DTensor(data);
            using var gpuPadded = gpuTensor.AddPadding(1);
            FloatMath.AreApproximatelyEqual(cpuPadded.AsIndexable(), gpuPadded.AsIndexable());
        }

		[Fact]
		public void Tensor4DRemovePadding()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuPadded = cpuTensor.RemovePadding(1);
            using var gpuTensor = _cuda.Create4DTensor(data);
            using var gpuPadded = gpuTensor.RemovePadding(1);
            FloatMath.AreApproximatelyEqual(cpuPadded.AsIndexable(), gpuPadded.AsIndexable());
        }

		[Fact]
		public void TensorCombineDepthSlices()
		{
			var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var cpuTensor = _cpu.Create3DTensor(tensor);
			var cpuCombined = cpuTensor.CombineDepthSlices();

            using var gpuTensor = _cuda.Create3DTensor(tensor);
            using var gpuCombined = gpuTensor.CombineDepthSlices();
            FloatMath.AreApproximatelyEqual(cpuCombined.AsIndexable(), gpuCombined.AsIndexable());
		}

		[Fact]
		public void TensorCombineDepthSlices2()
		{
			var tensor = CheckCreateTensor(12, 6, 3, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var cpuTensor = _cpu.Create3DTensor(tensor);
			var cpuCombined = cpuTensor.CombineDepthSlices();

            using var gpuTensor = _cuda.Create3DTensor(tensor);
            using var gpuCombined = gpuTensor.CombineDepthSlices();
            FloatMath.AreApproximatelyEqual(cpuCombined.AsIndexable(), gpuCombined.AsIndexable());
		}

		[Fact]
		public void TensorAddInPlace()
		{
			var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var tensor2 = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var cpuTensor = _cpu.Create3DTensor(tensor);
			var cpuTensor2 = _cpu.Create3DTensor(tensor2);
			cpuTensor.AddInPlace(cpuTensor2);

            using var gpuTensor = _cuda.Create3DTensor(tensor);
            using var gpuTensor2 = _cuda.Create3DTensor(tensor2);
            gpuTensor.AddInPlace(gpuTensor2);
            FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), gpuTensor.AsIndexable());
        }

		[Fact]
		public void Tensor4DMaxPool()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuPooled = cpuTensor.MaxPool(2, 2, 2, 2, false).Result;

            using var gpuTensor = _cuda.Create4DTensor(data);
            using var gpuPooled = gpuTensor.MaxPool(2, 2, 2, 2, false).Result;
            FloatMath.AreApproximatelyEqual(cpuPooled.AsIndexable(), gpuPooled.AsIndexable());
        }

		[Fact]
		public void Tensor4DReverseMaxPool()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuResult = cpuTensor.MaxPool(2, 2, 2, 2, true);
			var cpuReverseMaxPool = cpuResult.Result.ReverseMaxPool(cpuResult.Indices, 4, 4, 2, 2, 2, 2);

            using var gpuTensor = _cuda.Create4DTensor(data);
            var gpuResult = gpuTensor.MaxPool(2, 2, 2, 2, true);
            using var gpuReverseMaxPool = gpuResult.Result.ReverseMaxPool(gpuResult.Indices, 4, 4, 2, 2, 2, 2);
            FloatMath.AreApproximatelyEqual(cpuReverseMaxPool.AsIndexable(), gpuReverseMaxPool.AsIndexable());
        }

		[Fact]
		public void Tensor4DIm2Col()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1) * (z + 1))).ToArray();
			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuResult = cpuTensor.Im2Col(2, 2, 1, 1);

            using var gpuTensor = _cuda.Create4DTensor(data);
            using var gpuResult = gpuTensor.Im2Col(2, 2, 1, 1);
            FloatMath.AreApproximatelyEqual(cpuResult.AsIndexable(), gpuResult.AsIndexable());
        }

		[Fact]
		public void TensorMultiplyMatrix()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(4, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuIm2Col = cpuTensor.Im2Col(2, 2, 1, 1);
			var cpuFilter = _cpu.CreateMatrix(2 * 2 * 2, 5, (i, j) => (i + 1) * (j + 1));
			var cpuOutput = cpuIm2Col.Multiply(cpuFilter);

            using var gpuIm2Col = _cuda.Create3DTensor(cpuIm2Col.Data);
            using var gpuFilter = _cuda.CreateMatrix(cpuFilter.AsIndexable());
            using var gpuOutput = gpuIm2Col.Multiply(gpuFilter);
            FloatMath.AreApproximatelyEqual(cpuOutput.AsIndexable(), gpuOutput.AsIndexable());
        }

		[Fact]
		public void Tensor3DToVector()
		{
			var data = CheckCreateTensor(4, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var tensor = _cpu.Create3DTensor(data);
			var raw = data.GetAsRaw();
			var vector = _cpu.CreateVector(raw);
			var tensor2 = vector.ReshapeAs3DTensor(4, 3, 2);
			FloatMath.AreApproximatelyEqual(tensor.AsIndexable(), tensor2.AsIndexable());

            using var gpuTensor = _cuda.Create3DTensor(tensor.Data);
            using var gpuVector = _cuda.CreateVector(raw);
            using var gpuTensor2 = gpuVector.ReshapeAs3DTensor(4, 3, 2);
            FloatMath.AreApproximatelyEqual(gpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
        }

		[Fact]
		public void Tensor3DToVector2()
		{
			var data = CheckCreateTensor(4, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var tensor = _cpu.Create3DTensor(data);
			var vector = tensor.ReshapeAsVector();
			var tensor2 = vector.ReshapeAs3DTensor(4, 3, 2);
			FloatMath.AreApproximatelyEqual(tensor.AsIndexable(), tensor2.AsIndexable());

            using var gpuTensor = _cuda.Create3DTensor(tensor.Data);
            using var gpuVector = gpuTensor.ReshapeAsVector();
            using var gpuTensor2 = gpuVector.ReshapeAs3DTensor(4, 3, 2);
            FloatMath.AreApproximatelyEqual(gpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
            FloatMath.AreApproximatelyEqual(vector.AsIndexable(), gpuVector.AsIndexable());
        }

		[Fact]
		public void Tensor3DToMatrix()
		{
			var data = CheckCreateTensor(4, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var tensor = _cpu.Create3DTensor(data);
			var matrix = tensor.ReshapeAsMatrix();
			var tensor2 = matrix.ReshapeAs3DTensor(4, 3);
			FloatMath.AreApproximatelyEqual(tensor.AsIndexable(), tensor2.AsIndexable());

            using var gpuTensor = _cuda.Create3DTensor(tensor.Data);
            using var gpuMatrix = gpuTensor.ReshapeAsMatrix();
            using var gpuTensor2 = gpuMatrix.ReshapeAs3DTensor(4, 3);
            FloatMath.AreApproximatelyEqual(gpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
            FloatMath.AreApproximatelyEqual(matrix.AsIndexable(), gpuMatrix.AsIndexable());
        }

		[Fact]
		public void Tensor3DToTensor4D()
		{
			var data = CheckCreateTensor(12, 3, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var tensor = _cpu.Create3DTensor(data);
			var tensor2 = tensor.ReshapeAs4DTensor(3, 4);

            using var gpuTensor = _cuda.Create3DTensor(tensor.Data);
            using var gpuTensor2 = gpuTensor.ReshapeAs4DTensor(3, 4);
            FloatMath.AreApproximatelyEqual(tensor2.AsIndexable(), gpuTensor2.AsIndexable());
        }

		[Fact]
		public void Tensor4DToMatrix()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1) * (z + 1))).ToArray();
			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuMatrix = cpuTensor.ReshapeAsMatrix();
			var cpuTensor2 = cpuMatrix.ReshapeAs4DTensor(3, 4, 2);
			FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), cpuTensor2.AsIndexable());

            using var gpuTensor = _cuda.Create4DTensor(cpuTensor.Data);
            using var gpuMatrix = gpuTensor.ReshapeAsMatrix();
            using var gpuTensor2 = gpuMatrix.ReshapeAs4DTensor(3, 4, 2);
            FloatMath.AreApproximatelyEqual(gpuTensor.AsIndexable(), gpuTensor2.AsIndexable());
            FloatMath.AreApproximatelyEqual(cpuMatrix.AsIndexable(), gpuMatrix.AsIndexable());
        }

		[Fact]
		public void Tensor3DAddVectorToEachRow()
		{
			var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var cpuTensor = _cpu.Create3DTensor(tensor);
			var cpuVector = _cpu.CreateVector(4, 1f);
			cpuTensor.AddToEachRow(cpuVector);

            using var gpuTensor = _cuda.Create3DTensor(tensor);
            using var gpuVector = _cuda.CreateVector(4, 1f);
            gpuTensor.AddToEachRow(gpuVector);
            FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), gpuTensor.AsIndexable());
        }

		[Fact]
		public void Tensor3DToFloatTensor()
		{
			var tensor = CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1));
			var cpuTensor = _cpu.Create3DTensor(tensor);
			FloatMath.AreApproximatelyEqual(cpuTensor.Data, tensor);

            using var gpuTensor = _cuda.Create3DTensor(tensor);
            FloatMath.AreApproximatelyEqual(cpuTensor.Data, gpuTensor.Data);
        }

		[Fact]
		public void Tensor3DTransposeThisAndMultiply()
		{
			var normalDistribution = new Normal(0, 1);
			var tensor1 = CheckCreateTensor(9, 3, 3, (i, j, k) => (float)normalDistribution.Sample());
			var data = Enumerable.Range(0, 3)
				.Select(z => CheckCreateTensor(3, 3, 3, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();

			var cpuTensor1 = _cpu.Create3DTensor(tensor1);
			var cpuTensor2 = _cpu.Create4DTensor(data);
			var cpuResult = cpuTensor1.TransposeThisAndMultiply(cpuTensor2);

            using var gpuTensor1 = _cuda.Create3DTensor(tensor1);
            using var gpuTensor2 = _cuda.Create4DTensor(data);
            var gpuResult = gpuTensor1.TransposeThisAndMultiply(gpuTensor2);
            FloatMath.AreApproximatelyEqual(cpuResult.AsIndexable(), gpuResult.AsIndexable());
        }

		[Fact]
		public void Tensor4DCounts()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
			var cpuTensor = _cpu.Create4DTensor(data);

            cpuTensor.Count.Should().Be(5);
            cpuTensor.Depth.Should().Be(2);
            cpuTensor.ColumnCount.Should().Be(4);
            cpuTensor.RowCount.Should().Be(3);

            using var gpuTensor = _cuda.Create4DTensor(data);
            cpuTensor.Count.Should().Be(gpuTensor.Count);
            cpuTensor.Depth.Should().Be(gpuTensor.Depth);
            cpuTensor.ColumnCount.Should().Be(gpuTensor.ColumnCount);
            cpuTensor.RowCount.Should().Be(gpuTensor.RowCount);
        }

		[Fact]
		public void Tensor4DColumnSums()
		{
			var data = Enumerable.Range(0, 5)
				.Select(z => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuResult = cpuTensor.ColumnSums();

            using var gpuTensor = _cuda.Create4DTensor(data);
            using var gpuResult = gpuTensor.ColumnSums();
            FloatMath.AreApproximatelyEqual(cpuResult.AsIndexable(), gpuResult.AsIndexable());
        }

		[Fact]
		public void Tensor4DGetTensorAt()
		{
			var data = 5.AsRange()
				.Select(z => CheckCreateTensor(3, 4, 2, (i, j, k) => (i + 1) * (j + 1) * (k + 1))).ToArray();
			var cpu4dTensor = _cpu.Create4DTensor(data);

            using var gpu4dTensor = _cuda.Create4DTensor(data);
            for (uint i = 0; i < 5; i++) {
                var cpuTensor = cpu4dTensor.GetTensorAt(i);
                var gpuTensor = gpu4dTensor.GetTensorAt(i);
                FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), gpuTensor.AsIndexable());
            }
        }

		[Fact]
		public void Tensor4DReverseIm2Col()
		{
			const int rows = 4, columns = 4, depth = 1, count = 1, filterWidth = 2, filterHeight = 2, filterCount = 1, xStride = 2, yStride = 2;

			var normalDistribution = new Normal(0, 1);
			var data = Enumerable.Range(0, count)
				.Select(z => CheckCreateTensor(rows, columns, depth, (i, j, k) => (float)normalDistribution.Sample())).ToArray();
			var cpuTensor = _cpu.Create4DTensor(data);
			var cpuFilter = _cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (i, j) => (float)normalDistribution.Sample());

            using var gpuTensor = _cuda.Create4DTensor(data);
            using var gpuFilter = _cuda.CreateMatrix(cpuFilter.AsIndexable());
            FloatMath.AreApproximatelyEqual(cpuTensor.AsIndexable(), gpuTensor.AsIndexable());

            var cpuReverseIm2Col = cpuTensor.ReverseIm2Col(cpuFilter, rows, columns, depth, filterWidth, filterHeight, xStride, yStride);
            using var gpuReverseIm2Col = gpuTensor.ReverseIm2Col(gpuFilter, rows, columns, depth, filterWidth, filterHeight, xStride, yStride);
            var cpuResult = cpuReverseIm2Col.AsIndexable();
            var gpuResult = gpuReverseIm2Col.AsIndexable();
            var cpuXml = cpuResult.AsXml;
            var gpuXml = gpuResult.AsXml;
            FloatMath.AreApproximatelyEqual(cpuResult, gpuResult);
        }
	}
}
