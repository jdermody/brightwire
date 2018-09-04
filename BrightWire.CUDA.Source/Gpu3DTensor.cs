using System;
using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using System.Diagnostics;
using System.Threading;
using BrightWire.CUDA.Source.Helper;
using ManagedCuda;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// GPU backed 3D tensor
    /// </summary>
    class Gpu3DTensor : I3DTensor, IHaveDeviceMemory
    {
	    readonly CudaProvider _cuda;
	    readonly IDeviceMemoryPtr _data;
        readonly int _rows, _columns, _depth, _blockSize;
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

        public Gpu3DTensor(CudaProvider provider, int rows, int columns, int depth, IDeviceMemoryPtr data, bool isOwner)
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
        ~Gpu3DTensor()
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

	    public IDeviceMemoryPtr Memory => _data;

	    public IEnumerable<IMatrix> Matrices 
	    {
		    get 
		    {
			    var i = 0;
			    while (i < Depth) {
				    yield return GetMatrixAt(i++);
			    }
		    }
	    }

        public FloatTensor Data
        {
            get
            {
                Debug.Assert(IsValid);
                return FloatTensor.Create(Matrices.Select(m => m.Data).ToArray());
            }
            set {
                Debug.Assert(IsValid);
                var matrixList = value.Matrix;
                var matrixCount = matrixList.Length;
                for (var i = 0; i < matrixCount && i < _depth; i++) {
                    var matrix = matrixList[i];
                    if (matrix.Row != null)
                        GetMatrixAt(i).Data = matrix;
                }
            }
        }

        public IMatrix GetMatrixAt(int depth)
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _rows, _columns, _cuda.OffsetByBlock(_data, depth, _blockSize), false);
        }

        public IIndexable3DTensor AsIndexable()
        {
            Debug.Assert(IsValid);
            return _cuda.NumericsProvider
                .Create3DTensor(Matrices.Select(m => _cuda.NumericsProvider.CreateMatrix(m.Data)).ToList())
                .AsIndexable()
            ;
        }

        public IVector AsVector()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _data, false);
        }

        public IMatrix AsMatrix()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _blockSize, _depth, _data, false);
        }

		public I4DTensor As4DTensor(int rows, int columns)
		{
			Debug.Assert(IsValid && rows * columns == _rows);
			return new Gpu4DTensor(_cuda, rows, columns, _columns, _depth, _data, false);
		}

		public I3DTensor AddPadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorAddPadding(_data, _rows, _columns, _depth, 1, padding);
	        return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, Depth, ret.Data, true);
        }

        public I3DTensor RemovePadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorRemovePadding(_data, _rows, _columns, _depth, 1, padding);
	        return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, Depth, ret.Data, true);
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            Debug.Assert(IsValid);
	        var ret = _cuda.TensorIm2Col(_data, _rows, _columns, _depth, 1, filterWidth, filterHeight, stride);
	        return new GpuMatrix(_cuda, ret.Rows, ret.Columns, ret.Data, true);
        }

	    public I3DTensor ReverseIm2Col(IMatrix filter, int outputRows, int outputColumns, int outputDepth, int filterWidth, int filterHeight, int stride)
	    {
		    Debug.Assert(IsValid);
			var filterPtr = ((IHaveDeviceMemory)filter).Memory;
		    var ret = _cuda.TensorReverseIm2Col(_data, filterPtr, _rows, _columns, _depth, 1, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, stride);
		    return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, ret.Depth, ret.Data, true);
	    }

        public (I3DTensor Result, I3DTensor Indices) MaxPool(int filterWidth, int filterHeight, int stride, bool saveIndices)
        {
	        Debug.Assert(IsValid);
	        var maxPool = _cuda.TensorMaxPool(_data, _rows, _columns, _depth, 1, filterWidth, filterHeight, stride, saveIndices);
	        var ret = new Gpu3DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, maxPool.Data, true);
	        var indices = saveIndices ? new Gpu3DTensor(_cuda, maxPool.Rows, maxPool.Columns, _depth, maxPool.Indices, true) : null;
	        return (ret, indices);
        }

        public I3DTensor ReverseMaxPool(I3DTensor indices, int outputRows, int outputColumns, int filterWidth, int filterHeight, int stride)
        {
            Debug.Assert(IsValid);
	        var indicesPtr = ((IHaveDeviceMemory) indices).Memory;
            var ret = _cuda.TensorReverseMaxPool(_data, indicesPtr, _rows, _columns, _depth, 1, outputRows, outputColumns, filterWidth, filterHeight, stride);
            return new Gpu3DTensor(_cuda, outputRows, outputColumns, _depth, ret, true);
        }

        public IMatrix CombineDepthSlices()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.CreateMatrix(_rows, _columns, true);
            foreach (var item in Matrices)
                ret.AddInPlace(item);
            return ret;
        }

        public void AddInPlace(I3DTensor tensor)
        {
            var other = (Gpu3DTensor)tensor;
            Debug.Assert(IsValid && other.IsValid);
            for (var i = 0; i < _depth; i++)
                GetMatrixAt(i).AddInPlace(other.GetMatrixAt(i));
        }

        public I3DTensor Multiply(IMatrix matrix)
        {
	        return _cuda.Create3DTensor(Matrices.Select(item => item.Multiply(matrix)).ToList());
        }

        public void AddToEachRow(IVector vector)
        {
            foreach (var item in Matrices)
                item.AddToEachRow(vector);
        }

        public I3DTensor TransposeThisAndMultiply(I4DTensor tensor)
        {
#if DEBUG
            var other = (Gpu4DTensor)tensor;
            Debug.Assert(tensor.Count == Depth && IsValid && other.IsValid);
#endif
            var ret = new List<IMatrix>();
            for (var i = 0; i < tensor.Count; i++) {
                var multiplyWith = tensor.GetTensorAt(i).AsMatrix();
                var slice = GetMatrixAt(i);
                ret.Add(slice.TransposeThisAndMultiply(multiplyWith));
            }
            return _cuda.Create3DTensor(ret);
        }
    }
}
