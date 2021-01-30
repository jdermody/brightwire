using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BrightData.LinearAlgebra;

namespace BrightData.Cuda
{
    /// <summary>
    /// GPU backed 4D tensor
    /// </summary>
    internal class Cuda4DTensor : I4DFloatTensor, IHaveDeviceMemory
    {
	    readonly CudaProvider _cuda;
	    readonly IDeviceMemoryPtr _data;
        readonly uint _rows, _columns, _depth, _count, _blockSize;
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

	    public Cuda4DTensor(CudaProvider provider, uint rows, uint columns, uint depth, uint count, IDeviceMemoryPtr data, bool isOwner)
	    {
		    Debug.Assert(rows * columns * depth * count == data.Size);
		    _cuda = provider;
			_rows = rows;
		    _columns = columns;
			_depth = depth;
			_count = count;
			_data = data;
		    _blockSize = _rows * _columns * _depth;
			provider.Register(this);

#if DEBUG
			if (_id == _badAlloc)
				Debugger.Break();
#endif
		}

#if DEBUG
		~Cuda4DTensor()
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
		    return $"4D tensor (GPU), rows:{RowCount} columns:{ColumnCount} depth:{Depth} count:{Count}";
	    }

        public ILinearAlgebraProvider LinearAlgebraProvider => _cuda.DataContext.LinearAlgebraProvider;
        public IDeviceMemoryPtr Memory => _data;
        public IFloatMatrix ReshapeAsMatrix() => new CudaMatrix(_cuda, _blockSize, _count, _data, false);
	    public Tensor3D<float>[] Data {
		    get
		    {
			    Debug.Assert(IsValid);
			    return Tensors.Select(m => m.Data).ToArray();
		    }
		    set {
			    Debug.Assert(IsValid);
			    var count = value.Length;
			    for (uint i = 0; i < count && i < _depth; i++) {
				    var tensor = value[(int)i];
				    if (tensor != null)
					    GetTensorAt(i).Data = tensor;
			    }
		    }
	    }
	    public IFloatVector ReshapeAsVector() => new CudaVector(_cuda, _data, false);

	    public IEnumerable<I3DFloatTensor> Tensors
	    {
		    get 
		    {
			    uint i = 0;
			    while (i < Count) {
				    yield return GetTensorAt(i++);
			    }
		    }
	    }

        public I3DFloatTensor GetTensorAt(uint index)
        {
            return new Cuda3DTensor(_cuda, _rows, _columns, _depth, _cuda.OffsetByBlock(_data, index, _blockSize), false);
        }

        public IIndexable4DFloatTensor AsIndexable()
        {
            var ret = new I3DFloatTensor[_count];
            for(uint i = 0; i < _count; i++)
                ret[i] = GetTensorAt(i).AsIndexable();
	        return _cuda.NumericsProvider.Create4DTensor(ret).AsIndexable();
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

        public uint Count
        {
            get
            {
                Debug.Assert(IsValid);
                return _count;
            }
        }

        public I4DFloatTensor AddPadding(uint padding)
        {
	        Debug.Assert(IsValid);
            var ret = _cuda.TensorAddPadding(_data, _rows, _columns, _depth, _count, padding);
	        return new Cuda4DTensor(_cuda, ret.Rows, ret.Columns, _depth, _count, ret.Data, true);
        }

        public I4DFloatTensor RemovePadding(uint padding)
        {
	        Debug.Assert(IsValid);
            var ret = _cuda.TensorRemovePadding(_data, _rows, _columns, _depth, _count, padding);
	        return new Cuda4DTensor(_cuda, ret.Rows, ret.Columns, _depth, _count, ret.Data, true);
        }

        public (I4DFloatTensor Result, I4DFloatTensor Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
	        Debug.Assert(IsValid);
	        var maxPool = _cuda.TensorMaxPool(_data, _rows, _columns, _depth, _count, filterWidth, filterHeight, xStride, yStride, saveIndices);
	        var ret = new Cuda4DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, _count, maxPool.Data, true);
	        var indices = saveIndices ? new Cuda4DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, _count, maxPool.Indices, true) : null;
	        return (ret, indices);
        }

        public I4DFloatTensor ReverseMaxPool(I4DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
	        Debug.Assert(IsValid);
	        var indicesPtr = ((IHaveDeviceMemory) indices).Memory;
	        var ret = _cuda.TensorReverseMaxPool(_data, indicesPtr, _rows, _columns, _depth, _count, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
	        return new Cuda4DTensor(_cuda, outputRows, outputColumns, _depth, _count, ret, true);
        }

        public I3DFloatTensor Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
			var ret = _cuda.TensorIm2Col(_data, _rows, _columns, _depth, _count, filterWidth, filterHeight, xStride, yStride);
	        return new Cuda3DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Data, true);
        }

        public I4DFloatTensor ReverseIm2Col(IFloatMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
	        var filterPtr = ((IHaveDeviceMemory) filter).Memory;
			var ret = _cuda.TensorReverseIm2Col(_data, filterPtr, _rows, _columns, _depth, _count, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
			return new Cuda4DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Count, ret.Data, true);
		}

		public IFloatVector ColumnSums()
        {
            IFloatVector ret = null;
            for (uint i = 0; i < Count; i++) {
                var tensorAsMatrix = GetTensorAt(i).ReshapeAsMatrix();
                var columnSums = tensorAsMatrix.ColumnSums();
                if (ret == null)
                    ret = columnSums;
                else
                    ret.AddInPlace(columnSums);
            }
            return ret;
        }
    }
}
