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
	    readonly bool _isOwner;
        readonly int _rows, _columns, _depth, _count, _blockSize;
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

	    public Gpu4DTensor(CudaProvider provider, int rows, int columns, int depth, int count, IDeviceMemoryPtr data, bool isOwner)
	    {
		    Debug.Assert(rows * columns * depth * count == data.Size);
		    _cuda = provider;
			_rows = rows;
		    _columns = columns;
			_depth = depth;
			_count = count;
			_data = data;
		    _isOwner = isOwner;
		    _blockSize = _rows * _columns * _depth;
			//var firstTensor = data.First();
			//var firstMatrix = firstTensor.Matrix.First();

			//_cuda = provider;
			//_rows = firstMatrix.RowCount;
			//_columns = firstMatrix.ColumnCount;
			//_depth = firstTensor.Matrix.Length;
			//_count = data.Count;

			//var matrixSize = _rows * _columns;
			//_data = provider.CreateMatrix(matrixSize * _depth, _count, (index, k) => {
			//	var z = index / matrixSize;
			//	var rem = index % matrixSize;
			//	var i = rem / _rows;
			//	var j = rem % _rows;
			//	return data[k].Matrix[z].Row[j].Data[i];
			//});
			provider.Register(this);

			//_subVector = new Lazy<List<GpuVector[]>>(_GetSubVectors);
			//_tensorInfo = new Lazy<Tensor4DInput>(_GetInput);

#if DEBUG
			if (_id == _badAlloc)
				Debugger.Break();
#endif
		}

		//        public Gpu4DTensor(CudaProvider provider, IReadOnlyList<FloatTensor> data)
		//        {
		//            var firstTensor = data.First();
		//            var firstMatrix = firstTensor.Matrix.First();

		//            _cuda = provider;
		//            _rows = firstMatrix.RowCount;
		//            _columns = firstMatrix.ColumnCount;
		//            _depth = firstTensor.Matrix.Length;
		//            _count = data.Count;

		//            var matrixSize = _rows * _columns;
		//            _data = provider.CreateMatrix(matrixSize * _depth, _count, (index, k) => {
		//                var z = index / matrixSize;
		//                var rem = index % matrixSize;
		//                var i = rem / _rows;
		//                var j = rem % _rows;
		//                return data[k].Matrix[z].Row[j].Data[i];
		//            });
		//            provider.Register(this);

		//            _subVector = new Lazy<List<GpuVector[]>>(_GetSubVectors);
		//            _tensorInfo = new Lazy<Tensor4DInput>(_GetInput);

		//#if DEBUG
		//            if (_id == _badAlloc)
		//                Debugger.Break();
		//#endif
		//        }

		//        internal Gpu4DTensor(CudaProvider provider, IMatrix data, int rows, int columns, int depth)
		//        {
		//            _cuda = provider;
		//            _data = data;
		//            _rows = rows;
		//            _columns = columns;
		//            _depth = depth;
		//            _count = data.ColumnCount;
		//            provider.Register(this);

		//            _subVector = new Lazy<List<GpuVector[]>>(_GetSubVectors);
		//            _tensorInfo = new Lazy<Tensor4DInput>(_GetInput);

		//#if DEBUG
		//            if (_id == _badAlloc)
		//                Debugger.Break();
		//#endif
		//        }

		//        internal Gpu4DTensor(CudaProvider provider, IReadOnlyList<I3DTensor> tensorList)
		//        {
		//            var first = tensorList.First();
		//            _cuda = provider;
		//            _rows = first.RowCount;
		//            _columns = first.ColumnCount;
		//            _depth = first.Depth;
		//            _count = tensorList.Count;
		//            provider.Register(this);

		//            _data = _cuda.CreateZeroMatrix(_rows * _columns * _depth, _count);
		//            _subVector = new Lazy<List<GpuVector[]>>(_GetSubVectors);
		//            _tensorInfo = new Lazy<Tensor4DInput>(_GetInput);

		//            for (var i = 0; i < _count; i++)
		//                _data.Column(i).AddInPlace(tensorList[i].ConvertToVector());

		//#if DEBUG
		//            if (_id == _badAlloc)
		//                Debugger.Break();
		//#endif
		//        }

#if DEBUG
		~Gpu4DTensor()
        {
            if (_isOwner && !_disposed)
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
				if(_isOwner)
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

		public int BlockSize => _blockSize;
	    public bool IsOwner => _isOwner;
	    public CudaDeviceVariable<float> CudaDeviceVariable => _data.DeviceVariable;
	    public IDeviceMemoryPtr Memory => _data;

        public IMatrix AsMatrix() => new GpuMatrix(_cuda, _blockSize, _count, _data, false);
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
	    public IVector AsVector() => new GpuVector(_cuda, _data, false);

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

        public I3DTensor GetTensorAt(int index)
        {
            return new Gpu3DTensor(_cuda, _rows, _columns, _depth, _cuda.OffsetByBlock(_data, index, _blockSize), false);
        }

        public IReadOnlyList<IIndexable3DTensor> AsIndexable()
        {
            var ret = new List<IIndexable3DTensor>();
            for(var i = 0; i < _count; i++)
                ret.Add(GetTensorAt(i).AsIndexable());
            return ret;
        }

        public int ColumnCount
        {
            get
            {
                Debug.Assert(IsValid);
                return _columns;
            }
        }

        public int Depth
        {
            get
            {
                Debug.Assert(IsValid);
                return _depth;
            }
        }

        public int RowCount
        {
            get
            {
                Debug.Assert(IsValid);
                return _rows;
            }
        }

        public int Count
        {
            get
            {
                Debug.Assert(IsValid);
                return _count;
            }
        }

        public I4DTensor AddPadding(int padding)
        {
	        Debug.Assert(IsValid);
            var ret = _cuda.TensorAddPadding(_data, _rows, _columns, _depth, _count, padding);
	        return new Gpu4DTensor(_cuda, ret.Rows, ret.Columns, _depth, _count, ret.Data, true);
        }

        public I4DTensor RemovePadding(int padding)
        {
	        Debug.Assert(IsValid);
            var ret = _cuda.TensorRemovePadding(_data, _rows, _columns, _depth, _count, padding);
	        return new Gpu4DTensor(_cuda, ret.Rows, ret.Columns, _depth, _count, ret.Data, true);
        }

        public (I4DTensor Result, I4DTensor Indices) MaxPool(int filterWidth, int filterHeight, int stride, bool saveIndices)
        {
	        Debug.Assert(IsValid);
	        var maxPool = _cuda.TensorMaxPool(_data, _rows, _columns, _depth, _count, filterWidth, filterHeight, stride, saveIndices);
	        var ret = new Gpu4DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, _count, maxPool.Data, true);
	        var indices = saveIndices ? new Gpu4DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, _count, maxPool.Indices, true) : null;
	        return (ret, indices);
        }

        public I4DTensor ReverseMaxPool(I4DTensor indices, int outputRows, int outputColumns, int filterWidth, int filterHeight, int stride)
        {
	        Debug.Assert(IsValid);
	        var indicesPtr = ((IHaveDeviceMemory) indices).Memory;
	        var ret = _cuda.TensorReverseMaxPool(_data, indicesPtr, _rows, _columns, _depth, _count, outputRows, outputColumns, filterWidth, filterHeight, stride);
	        return new Gpu4DTensor(_cuda, outputRows, outputColumns, _depth, _count, ret, true);
        }

        public I3DTensor Im2Col(int filterWidth, int filterHeight, int stride)
        {
			var ret = _cuda.TensorIm2Col(_data, _rows, _columns, _depth, _count, filterWidth, filterHeight, stride);
	        return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Data, true);
        }

        public I3DTensor ReverseIm2Col(IReadOnlyList<IReadOnlyList<IVector>> filter, int inputHeight, int inputWidth, int padding, int filterWidth, int filterHeight, int stride)
        {
			var filters = filter.Select(fl => fl.Cast<GpuVector>().Select(v => v.Memory).ToList()).ToList();
			var ret = _cuda.TensorReverseIm2Col(_data, filters, _rows, _columns, _depth, _count, inputHeight, inputWidth, padding, filterWidth, filterHeight, stride);
	        return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Data, true);
		}

		public IVector ColumnSums()
        {
            IVector ret = null;
            for (var i = 0; i < Count; i++) {
                var tensorAsMatrix = GetTensorAt(i).AsMatrix();
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
