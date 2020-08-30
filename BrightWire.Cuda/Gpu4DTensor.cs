using BrightWire.Cuda.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ManagedCuda;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// GPU backed 4D tensor
    /// </summary>
    class Gpu4DTensor : I4DTensor, IHaveDeviceMemory
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

	    public Gpu4DTensor(CudaProvider provider, uint rows, uint columns, uint depth, uint count, IDeviceMemoryPtr data, bool isOwner)
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
		~Gpu4DTensor()
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

	    public IDeviceMemoryPtr Memory => _data;
        public IMatrix ReshapeAsMatrix() => new GpuMatrix(_cuda, _blockSize, _count, _data, false);
	    public IReadOnlyList<FloatTensor> Data {
		    get
		    {
			    Debug.Assert(IsValid);
			    return Tensors.Select(m => m.Data).ToList();
		    }
		    set {
			    Debug.Assert(IsValid);
			    var count = value.Count;
			    for (var i = 0; i < count && i < _depth; i++) {
				    var tensor = value[i];
				    if (tensor != null)
					    GetTensorAt(i).Data = tensor;
			    }
		    }
	    }
	    public IVector ReshapeAsVector() => new GpuVector(_cuda, _data, false);

	    public IEnumerable<I3DTensor> Tensors
	    {
		    get 
		    {
			    var i = 0;
			    while (i < Count) {
				    yield return GetTensorAt(i++);
			    }
		    }
	    }

        public I3DTensor GetTensorAt(uint index)
        {
            return new Gpu3DTensor(_cuda, _rows, _columns, _depth, _cuda.OffsetByBlock(_data, index, _blockSize), false);
        }

        public IIndexable4DTensor AsIndexable()
        {
            var ret = new List<IIndexable3DTensor>();
            for(var i = 0; i < _count; i++)
                ret.Add(GetTensorAt(i).AsIndexable());
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

        public I4DTensor AddPadding(uint padding)
        {
	        Debug.Assert(IsValid);
            var ret = _cuda.TensorAddPadding(_data, _rows, _columns, _depth, _count, padding);
	        return new Gpu4DTensor(_cuda, ret.Rows, ret.Columns, _depth, _count, ret.Data, true);
        }

        public I4DTensor RemovePadding(uint padding)
        {
	        Debug.Assert(IsValid);
            var ret = _cuda.TensorRemovePadding(_data, _rows, _columns, _depth, _count, padding);
	        return new Gpu4DTensor(_cuda, ret.Rows, ret.Columns, _depth, _count, ret.Data, true);
        }

        public (I4DTensor Result, I4DTensor Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
	        Debug.Assert(IsValid);
	        var maxPool = _cuda.TensorMaxPool(_data, _rows, _columns, _depth, _count, filterWidth, filterHeight, xStride, yStride, saveIndices);
	        var ret = new Gpu4DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, _count, maxPool.Data, true);
	        var indices = saveIndices ? new Gpu4DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, _count, maxPool.Indices, true) : null;
	        return (ret, indices);
        }

        public I4DTensor ReverseMaxPool(I4DTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
	        Debug.Assert(IsValid);
	        var indicesPtr = ((IHaveDeviceMemory) indices).Memory;
	        var ret = _cuda.TensorReverseMaxPool(_data, indicesPtr, _rows, _columns, _depth, _count, outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
	        return new Gpu4DTensor(_cuda, outputRows, outputColumns, _depth, _count, ret, true);
        }

        public I3DTensor Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
			var ret = _cuda.TensorIm2Col(_data, _rows, _columns, _depth, _count, filterWidth, filterHeight, xStride, yStride);
	        return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Data, true);
        }

        public I4DTensor ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
	        var filterPtr = ((IHaveDeviceMemory) filter).Memory;
			var ret = _cuda.TensorReverseIm2Col(_data, filterPtr, _rows, _columns, _depth, _count, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
			return new Gpu4DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Count, ret.Data, true);
		}

		public IVector ColumnSums()
        {
            IVector ret = null;
            for (var i = 0; i < Count; i++) {
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
