using BrightWire.CUDA.Helper;
using BrightWire.LinearAlgebra;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// GPU backed 4D tensor
    /// </summary>
    class Gpu4DTensor : I4DTensor
    {
        readonly IMatrix _data;
        readonly Lazy<List<GpuVector[]>> _subVector;
        readonly Lazy<TensorInput> _tensorInfo;
        readonly int _rows, _columns, _depth, _count;
        readonly CudaProvider _cuda;
        bool _disposed = false;

#if DEBUG
        static int _gid = 0;
        static int _GetNextIndex() => Interlocked.Increment(ref _gid);
        int _id = _GetNextIndex();
        public static int _badAlloc = -1;
        public static int _badDispose = -1;

        public bool IsValid => !_disposed;
#else
        public bool IsValid => true;
#endif

        public Gpu4DTensor(CudaProvider provider, IReadOnlyList<FloatTensor> data)
        {
            var firstTensor = data.First();
            var firstMatrix = firstTensor.Matrix.First();

            _cuda = provider;
            _rows = firstMatrix.RowCount;
            _columns = firstMatrix.ColumnCount;
            _depth = firstTensor.Matrix.Length;
            _count = data.Count;

            var matrixSize = _rows * _columns;
            _data = provider.CreateMatrix(matrixSize * _depth, _count, (index, k) => {
                var z = index / matrixSize;
                var rem = index % matrixSize;
                var i = rem / _rows;
                var j = rem % _rows;
                return data[k].Matrix[z].Row[j].Data[i];
            });
            provider.Register(this);

            _subVector = new Lazy<List<GpuVector[]>>(_GetSubVectors);
            _tensorInfo = new Lazy<TensorInput>(_GetInput);

#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

        internal Gpu4DTensor(CudaProvider provider, IMatrix data, int rows, int columns, int depth)
        {
            _cuda = provider;
            _data = data;
            _rows = rows;
            _columns = columns;
            _depth = depth;
            _count = data.ColumnCount;
            provider.Register(this);

            _subVector = new Lazy<List<GpuVector[]>>(_GetSubVectors);
            _tensorInfo = new Lazy<TensorInput>(_GetInput);

#if DEBUG
            if (_id == _badAlloc)
                Debugger.Break();
#endif
        }

        internal Gpu4DTensor(CudaProvider provider, IReadOnlyList<I3DTensor> tensorList)
        {
            var first = tensorList.First();
            _cuda = provider;
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
            _count = tensorList.Count;
            provider.Register(this);

            _data = _cuda.CreateZeroMatrix(_rows * _columns * _depth, _count);
            _subVector = new Lazy<List<GpuVector[]>>(_GetSubVectors);
            _tensorInfo = new Lazy<TensorInput>(_GetInput);

            for (var i = 0; i < _count; i++)
                _data.Column(i).AddInPlace(tensorList[i].ConvertToVector());

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
                _data.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
#if DEBUG
            GC.SuppressFinalize(this);
#endif
        }

        List<GpuVector[]> _GetSubVectors()
        {
            var ret = new List<GpuVector[]>();
            for (var i = 0; i < _data.ColumnCount; i++)
                ret.Add(_data.Column(i).Split(_depth).Cast<GpuVector>().ToArray());
            return ret;
        }

        TensorInput _GetInput()
        {
            return new TensorInput(_rows, _columns, _subVector.Value.Select(t => t.Select(m => m.Memory).ToArray()).ToList());
        }

        public I3DTensor GetTensorAt(int index)
        {
            var subMatrix = _subVector.Value;
            var ret = subMatrix[index]
                .Select(v => v.ConvertInPlaceToMatrix(_rows, _columns))
                .Cast<GpuMatrix>()
                .ToList()
            ;
            var tensor = new Gpu3DTensor(_cuda, _rows, _columns, _depth, ret);
            return tensor;
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
            var ret = _cuda.TensorAddPadding(_tensorInfo.Value, padding);
            return ret.GetAsTensor();
        }

        public I4DTensor RemovePadding(int padding)
        {
            var ret = _cuda.TensorRemovePadding(_tensorInfo.Value, padding);
            return ret.GetAsTensor();
        }

        public (I4DTensor Result, IReadOnlyList<IReadOnlyList<(object X, object Y)>> Index) MaxPool(int filterWidth, int filterHeight, int stride, bool calculateIndex)
        {
            List<IReadOnlyList<(object X, object Y)>> indexList = calculateIndex ? new List<IReadOnlyList<(object X, object Y)>>() : null;
            var ret = new List<I3DTensor>();
            for(var i = 0; i < _count; i++) {
                var result = GetTensorAt(i).MaxPool(filterWidth, filterHeight, stride, calculateIndex);
                if (calculateIndex)
                    indexList.Add(result.Index);
                ret.Add(result.Result);
            }
            return (new Gpu4DTensor(_cuda, ret), indexList);
        }

        public I4DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<IReadOnlyList<(object X, object Y)>> indexList)
        {
            var ret = new List<I3DTensor>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseMaxPool(rows, columns, indexList?[i]);
                ret.Add(result);
            }
            return new Gpu4DTensor(_cuda, ret);
        }

        public I3DTensor Im2Col(int filterWidth, int filterHeight, int stride)
        {
            var ret = _cuda.TensorIm2Col(_tensorInfo.Value, filterWidth, filterHeight, stride);
            var matrixList = ret.GetAsTensor().Cast<GpuMatrix>().ToList();
            return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, ret.Count, matrixList);
        }

        public I3DTensor ReverseIm2Col(IReadOnlyList<IReadOnlyList<IVector>> filter, int inputHeight, int inputWidth, int inputDepth, int padding, int filterWidth, int filterHeight, int stride)
        {
            var filters = filter.Select(fl => fl.Cast<GpuVector>().Select(v => v.Memory).ToList()).ToList();
            var ret = _cuda.TensorReverseIm2Col(_tensorInfo.Value, filters, inputHeight, inputWidth, inputDepth, padding, filterHeight, filterWidth, stride);
            var matrixList = new List<IMatrix>();
            for (var i = 0; i < Count; i++)
                matrixList.Add(ret.GetAt(i).CombineDepthSlices());
            return new Gpu3DTensor(_cuda, ret.Rows, ret.Columns, matrixList.Count, matrixList.Cast<GpuMatrix>().ToList());
        }

        public IMatrix ConvertToMatrix() => _data;
    }
}
