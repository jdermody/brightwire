using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BrightData.FloatTensors;
using ManagedCuda.CudaBlas;

namespace BrightData.Cuda
{
	/// <summary>
	/// GPU backed 3D tensor
	/// </summary>
	class Cuda3DTensor : I3DFloatTensor, IHaveDeviceMemory
	{
		readonly CudaProvider _cuda;
		readonly IDeviceMemoryPtr _data;
		readonly uint _rows, _columns, _depth, _blockSize;
		bool _disposed = false;

#if DEBUG
		static int _gid = 0;
		static int _GetNextIndex() => Interlocked.Increment(ref _gid);
		readonly int _id = _GetNextIndex();
		public static int _badAlloc = -1;
		public static int _badDispose = -1;

		public bool IsValid => !_disposed;
#else
        public bool IsValid => true;
#endif

		public Cuda3DTensor(CudaProvider provider, uint rows, uint columns, uint depth, IDeviceMemoryPtr data, bool isOwner)
		{
			Debug.Assert(rows * columns * depth == data.Size);
			_cuda = provider;
			_rows = rows;
			_columns = columns;
			_depth = depth;
			_data = data;
			_blockSize = rows * columns;
			provider.Register(this);

#if DEBUG
			if (_id == _badAlloc)
				Debugger.Break();
#endif
		}

#if DEBUG
		~Cuda3DTensor()
		{
			if (!_disposed)
				Debug.WriteLine("\tTensor {0} was not disposed !!", _id);
		}
#endif

		protected virtual void Dispose(bool disposing)
		{
#if DEBUG
			if (_id == _badDispose)
				Debugger.Break();
#endif
			if (!_disposed) {
				_disposed = true;
				_data.Free();
			}
		}

		public void Dispose()
		{
			Dispose(true);
#if DEBUG
			GC.SuppressFinalize(this);
#endif
		}

		public override string ToString()
		{
			return $"3D tensor (GPU), rows:{_rows} columns:{_columns} depth:{_depth}";
		}

		public uint ColumnCount
		{
			get
			{
				Debug.Assert(IsValid);
				return _columns;
			}
		}

		public uint Depth
		{
			get
			{
				Debug.Assert(IsValid);
				return _depth;
			}
		}

		public uint RowCount
		{
			get
			{
				Debug.Assert(IsValid);
				return _rows;
			}
		}

		public IDeviceMemoryPtr Memory => _data;
        public ILinearAlgebraProvider LinearAlgebraProvider => _cuda.DataContext.LinearAlgebraProvider;
		public IEnumerable<IFloatMatrix> Matrices
		{
			get
			{
				uint i = 0;
				while (i < Depth) {
					yield return GetMatrixAt(i++);
				}
			}
		}

		public Tensor3D<float> Data
		{
			get
			{
				Debug.Assert(IsValid);
				return Float3DTensor.Create(_cuda.DataContext, Matrices.Select(m => m.Data).ToArray());
			}
			set
			{
				Debug.Assert(IsValid);
				var matrixList = value.Matrices.ToList();
                for (uint i = 0, len = (uint)matrixList.Count; i < len && i < _depth; i++) {
					var matrix = matrixList[(int)i];
					GetMatrixAt(i).Data = matrix;
				}
			}
		}

		public IFloatMatrix GetMatrixAt(uint depth)
		{
			Debug.Assert(IsValid);
			return new CudaMatrix(_cuda, _rows, _columns, _cuda.OffsetByBlock(_data, depth, _blockSize), false);
		}

		public IIndexable3DFloatTensor AsIndexable()
		{
			Debug.Assert(IsValid);
			return _cuda.NumericsProvider
				.Create3DTensor(Matrices.Select(m => _cuda.NumericsProvider.CreateMatrix(m.Data)).ToArray())
				.AsIndexable()
			;
		}

		public IFloatVector ReshapeAsVector()
		{
			Debug.Assert(IsValid);
			return new CudaVector(_cuda, _data, false);
		}

		public IFloatMatrix ReshapeAsMatrix()
		{
			Debug.Assert(IsValid);
			return new CudaMatrix(_cuda, _blockSize, _depth, _data, false);
		}

		public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns)
		{
			Debug.Assert(IsValid && rows * columns == _rows);
			return new Cuda4DTensor(_cuda, rows, columns, _columns, _depth, _data, false);
		}

		public I3DFloatTensor AddPadding(uint padding)
		{
			Debug.Assert(IsValid);
			var ret = _cuda.TensorAddPadding(_data, _rows, _columns, _depth, 1, padding);
			return new Cuda3DTensor(_cuda, ret.Rows, ret.Columns, Depth, ret.Data, true);
		}

		public I3DFloatTensor RemovePadding(uint padding)
		{
			Debug.Assert(IsValid);
			var ret = _cuda.TensorRemovePadding(_data, _rows, _columns, _depth, 1, padding);
			return new Cuda3DTensor(_cuda, ret.Rows, ret.Columns, Depth, ret.Data, true);
		}

		public IFloatMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
		{
			Debug.Assert(IsValid);
			var ret = _cuda.TensorIm2Col(_data, _rows, _columns, _depth, 1, filterWidth, filterHeight, xStride, yStride);
			return new CudaMatrix(_cuda, ret.Rows, ret.Columns, ret.Data, true);
		}

		public I3DFloatTensor ReverseIm2Col(IFloatMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
		{
			Debug.Assert(IsValid);
			var filterPtr = ((IHaveDeviceMemory)filter).Memory;
			var ret = _cuda.TensorReverseIm2Col(_data, filterPtr, _rows, _columns, _depth, 1, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
			return new Cuda3DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Data, true);
		}

		public (I3DFloatTensor Result, I3DFloatTensor Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
		{
			Debug.Assert(IsValid);
			var maxPool = _cuda.TensorMaxPool(_data, _rows, _columns, _depth, 1, filterWidth, filterHeight, xStride, yStride, saveIndices);
			var ret = new Cuda3DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, maxPool.Data, true);
			var indices = saveIndices ? new Cuda3DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, maxPool.Indices, true) : null;
			return (ret, indices);
		}

		public I3DFloatTensor ReverseMaxPool(I3DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
		{
			Debug.Assert(IsValid);
			var indicesPtr = ((IHaveDeviceMemory)indices).Memory;
			var ret = _cuda.TensorReverseMaxPool(_data, indicesPtr, _rows, _columns, _depth, 1, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
			return new Cuda3DTensor(_cuda, outputRows, outputColumns, _depth, ret, true);
		}

		public IFloatMatrix CombineDepthSlices()
		{
			Debug.Assert(IsValid);
			var ret = _cuda.CreateMatrix(_rows, _columns, true);
			foreach (var item in Matrices)
				ret.AddInPlace(item);
			return ret;
		}

		public void AddInPlace(I3DFloatTensor tensor)
		{
			var other = (Cuda3DTensor)tensor;
			Debug.Assert(IsValid && other.IsValid);
			for (uint i = 0; i < _depth; i++)
				GetMatrixAt(i).AddInPlace(other.GetMatrixAt(i));
		}

		public I3DFloatTensor Multiply(IFloatMatrix matrix)
		{
			var other = (CudaMatrix)matrix;
			var ptr = _data.DevicePointer;
			uint rowsA = _rows, columnsArowsB = _columns, columnsB = matrix.ColumnCount;
			float alpha = 1.0f, beta = 0.0f;
			var output = new Cuda3DTensor(_cuda, _rows, columnsB, _depth, _cuda.Allocate(_rows * columnsB * _depth), true);

			var status = CudaBlasNativeMethods.cublasSgemmStridedBatched(_cuda.Blas.CublasHandle,
				Operation.NonTranspose,
				Operation.NonTranspose,
				(int)rowsA,
                (int)columnsB,
                (int)columnsArowsB,
				ref alpha,
				ptr,
                (int)rowsA,
				_blockSize,
				other.Memory.DevicePointer,
                (int)columnsArowsB,
				0,
				ref beta,
				output.Memory.DevicePointer,
                (int)rowsA,
				_rows * columnsB,
                (int)_depth
			);
			if (status != CublasStatus.Success)
				throw new CudaBlasException(status);

			return output;

			//var output = Enumerable.Range(0, _depth).Select(i => new GpuMatrix(_cuda, _rows, columnsB, _cuda.Allocate(_rows * columnsB), true)).ToList();

			//using (var aPtrs = new PtrToDeviceMemoryList(Enumerable.Range(0, _depth).Select(i => ptr + i * _blockSize * CudaProvider.FLOAT_SIZE).ToArray()))
			//using (var bPtrs = new PtrToDeviceMemoryList(Enumerable.Range(0, _depth).Select(i => other.Memory.DevicePointer).ToArray()))
			//using (var cPtrs = new PtrToDeviceMemoryList(output.Select(m => m.Memory.DevicePointer).ToArray())) {
			//	var status = CudaBlasNativeMethods.cublasSgemmBatched(_cuda.Blas.CublasHandle,
			//		Operation.NonTranspose,
			//		Operation.NonTranspose,
			//		rowsA,
			//		columnsB,
			//		columnsArowsB,
			//		ref alpha,
			//		aPtrs.DevicePointer,
			//		rowsA,
			//		bPtrs.DevicePointer,
			//		columnsArowsB,
			//		ref beta,
			//		cPtrs.DevicePointer,
			//		rowsA,
			//		_depth
			//	);
			//	if (status != CublasStatus.Success)
			//		throw new CudaBlasException(status);
			//}

			//return _cuda.Create3DTensor(output);
		}

		public void AddToEachRow(IFloatVector vector)
		{
			foreach (var item in Matrices)
				item.AddToEachRow(vector);
		}

		public I3DFloatTensor TransposeThisAndMultiply(I4DFloatTensor tensor)
		{
			var other = (Cuda4DTensor)tensor;
#if DEBUG
			Debug.Assert(tensor.Count == Depth && IsValid && other.IsValid);
#endif
			var ptr = _data.DevicePointer;
			var ptr2 = other.Memory.DevicePointer;
			uint rowsA = _rows, columnsA = _columns, columnsB = other.Depth, rowsB = other.RowCount * other.ColumnCount, blockSize2 = columnsB * rowsB;
			float alpha = 1.0f, beta = 0.0f;
			var output = new Cuda3DTensor(_cuda, _columns, columnsB, _depth, _cuda.Allocate(_columns * columnsB * _depth), true);

			var status = CudaBlasNativeMethods.cublasSgemmStridedBatched(_cuda.Blas.CublasHandle,
				Operation.Transpose,
				Operation.NonTranspose,
				(int)columnsA,
                (int)columnsB,
                (int)rowsB,
				ref alpha,
				ptr,
                (int)rowsA,
				_blockSize,
				ptr2,
                (int)rowsB,
				blockSize2,
				ref beta,
				output.Memory.DevicePointer,
                (int)columnsA,
				_columns * columnsB,
                (int)_depth
			);
			if (status != CublasStatus.Success)
				throw new CudaBlasException(status);

			return output;

			//var output = Enumerable.Range(0, _depth).Select(i => new GpuMatrix(_cuda, _columns, columnsB, _cuda.Allocate(_columns * columnsB), true)).ToList();

			//using (var aPtrs = new PtrToDeviceMemoryList(Enumerable.Range(0, _depth).Select(i => ptr + i * _blockSize * CudaProvider.FLOAT_SIZE).ToArray()))
			//using (var bPtrs = new PtrToDeviceMemoryList(Enumerable.Range(0, _depth).Select(i => ptr2 + i * blockSize2 * CudaProvider.FLOAT_SIZE).ToArray()))
			//using (var cPtrs = new PtrToDeviceMemoryList(output.Select(m => m.Memory.DevicePointer).ToArray())) {
			//	var status = CudaBlasNativeMethods.cublasSgemmBatched(_cuda.Blas.CublasHandle,
			//		Operation.Transpose,
			//		Operation.NonTranspose,
			//		columnsA,
			//		columnsB,
			//		rowsB,
			//		ref alpha,
			//		aPtrs.DevicePointer,
			//		rowsA,
			//		bPtrs.DevicePointer,
			//		rowsB,
			//		ref beta,
			//		cPtrs.DevicePointer,
			//		columnsA,
			//		_depth
			//	);
			//	if (status != CublasStatus.Success)
			//		throw new CudaBlasException(status);
			//}

			//return _cuda.Create3DTensor(output);
		}
	}
}
