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
	    readonly bool _isOwner;
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
            _cuda = provider;
            _rows = rows;
            _columns = columns;
            _depth = depth;
            _data = data;
	        _isOwner = isOwner;
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
				if(_isOwner)
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
			return AsIndexable().ToString();
		}

		public int BlockSize => _blockSize;
	    public bool IsOwner => _isOwner;

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

	    public CudaDeviceVariable<float> CudaDeviceVariable => _data.DeviceVariable;
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

        public IVector ConvertToVector()
        {
            Debug.Assert(IsValid);
            return new GpuVector(_cuda, _data, false);
        }

        public IMatrix ConvertToMatrix()
        {
            Debug.Assert(IsValid);
            return new GpuMatrix(_cuda, _blockSize, _depth, _data, false);
        }

        public I4DTensor ConvertTo4DTensor(int rows, int columns)
        {
            var tensorList = new List<I3DTensor>();
            for (var i = 0; i < Depth; i++) {
                var slice = GetMatrixAt(i);
                tensorList.Add(slice.ConvertTo3DTensor(rows, columns));
            }
            return _cuda.Create4DTensor(tensorList);
        }

        public I3DTensor AddPadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorAddPadding(_data, padding);
	        return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, Depth, ret.Data, true);
        }

        public I3DTensor RemovePadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorRemovePadding(_data, padding);
	        return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, Depth, ret.Data, true);
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            Debug.Assert(IsValid);
	        var ret = _cuda.TensorIm2Col(_data, _rows, _columns, _depth, 1, filterWidth, filterHeight, stride);
	        return new GpuMatrix(_cuda, ret.Rows, ret.Columns, ret.Data, true);
        }

	    public IMatrix ReverseIm2Col(IReadOnlyList<IReadOnlyList<IVector>> filter, int inputHeight, int inputWidth, int padding, int filterWidth, int filterHeight, int stride)
	    {
		    Debug.Assert(IsValid);
		    var filters = filter.Select(fl => fl.Cast<IHaveDeviceMemory>().Select(v => v.Memory).ToList()).ToList();
		    var ret = _cuda.TensorReverseIm2Col(_data, filters, _rows, _columns, _depth, 1, inputHeight, inputWidth, padding, filterWidth, filterHeight, stride);
		    return new GpuMatrix(_cuda, ret.Rows, ret.Columns, ret.Data, true);
	    }

        public (I3DTensor Result, IReadOnlyList<(object X, object Y)> Index) MaxPool(int filterWidth, int filterHeight, int stride, bool calculateIndex)
        {
	        throw new NotImplementedException();
	        //Debug.Assert(IsValid);
	        //var newColumns = (ColumnCount - filterWidth) / stride + 1;
	        //var newRows = (RowCount - filterHeight) / stride + 1;
	        //var data = _cuda.TensorMaxPool(_data, RowCount, ColumnCount, filterWidth, filterHeight, stride, calculateIndex);
	        //var ret = new Gpu3DTensor(_cuda, newRows, newColumns, data.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d.Item1)).ToList());

	        //List<(object X, object Y)> index = null;
	        //if(calculateIndex)
	        //    index = data.Select(d => (d.Item2, d.Item3)).ToList();
	        //return (ret, index);
        }

        public I3DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<(object X, object Y)> indexList)
        {
	        throw new NotImplementedException();
            //Debug.Assert(IsValid);
            //var ret = _cuda.TensorReverseMaxPool(_tensorInfo.Value.MatrixPtrList, RowCount, ColumnCount, rows, columns, indexList);
            //return new Gpu3DTensor(_cuda, rows, columns, ret.Select(d => new GpuMatrix(_cuda, rows, columns, d)).ToList());
        }

        public IMatrix CombineDepthSlices()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.CreateZeroMatrix(_rows, _columns);
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
                var multiplyWith = tensor.GetTensorAt(i).ConvertToMatrix();
                var slice = GetMatrixAt(i);
                ret.Add(slice.TransposeThisAndMultiply(multiplyWith));
            }
            return _cuda.Create3DTensor(ret);
        }
    }
}
