using System;
using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;
using System.Diagnostics;
using BrightWire.CUDA.Helper;
using System.Threading;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// GPU backed 3D tensor
    /// </summary>
    internal class Gpu3DTensor : I3DTensor
    {
        readonly IReadOnlyList<GpuMatrix> _data;
        readonly Lazy<TensorInput> _tensorInfo;
        readonly int _rows, _columns, _depth;
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

        public Gpu3DTensor(CudaProvider provider, int rows, int columns, int depth, IReadOnlyList<GpuMatrix> data)
        {
            _cuda = provider;
            _rows = rows;
            _columns = columns;
            _depth = depth;
            _data = data;
            _tensorInfo = new Lazy<TensorInput>(() => new TensorInput(rows, columns, new[] { data.Select(m => m.Memory).ToList() }));
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
                foreach (var item in _data)
                    item.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
#if DEBUG
            GC.SuppressFinalize(this);
#endif
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

        public FloatTensor Data
        {
            get
            {
                Debug.Assert(IsValid);
                return new FloatTensor {
                    Matrix = _data.Select(m => m.Data).ToArray()
                };
            }
            set {
                Debug.Assert(IsValid);
                var matrixList = value.Matrix;
                var matrixCount = matrixList.Length;
                for (var i = 0; i < matrixCount && i < _data.Count; i++) {
                    var matrix = matrixList[i];
                    if (matrix.Row != null)
                        _data[i].Data = matrix;
                }
            }
        }

        public IReadOnlyList<IMatrix> DepthSlices
        {
            get
            {
                Debug.Assert(IsValid);
                return _data;
            }
        }

        public IMatrix GetMatrixAt(int depth)
        {
            Debug.Assert(IsValid);
            return _data[depth];
        }

        public IIndexable3DTensor AsIndexable()
        {
            Debug.Assert(IsValid);
            return _cuda.NumericsProvider
                .Create3DTensor(_data.Select(m => _cuda.NumericsProvider.CreateMatrix(m.Data)).ToList())
                .AsIndexable()
            ;
        }

        public IVector ConvertToVector()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorConvertToVector(_tensorInfo.Value.Single(), _tensorInfo.Value.MatrixSize);
            return new GpuVector(_cuda, ret);
        }

        public IMatrix ConvertToMatrix()
        {
            Debug.Assert(IsValid);
            var rows = ColumnCount * RowCount;
            var columns = Depth;
            var ret = _cuda.Allocate(rows * columns);
            _cuda.TensorConvertToMatrix(_tensorInfo.Value.Single(), ColumnCount, RowCount, rows, columns, ret);
            return new GpuMatrix(_cuda, rows, columns, ret);
        }

        public I3DTensor AddPadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorAddPadding(_tensorInfo.Value, padding);
            return ret.Single();
        }

        public I3DTensor RemovePadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorRemovePadding(_tensorInfo.Value, padding);
            return ret.Single();
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorIm2Col(_tensorInfo.Value, filterWidth, filterHeight, stride);
            return ret.Single();
        }

        public (I3DTensor Result, IReadOnlyList<(object X, object Y)> Index) MaxPool(int filterWidth, int filterHeight, int stride, bool calculateIndex)
        {
            Debug.Assert(IsValid);
            var newColumns = (ColumnCount - filterWidth) / stride + 1;
            var newRows = (RowCount - filterHeight) / stride + 1;
            var data = _cuda.TensorMaxPool(_tensorInfo.Value.Single(), RowCount, ColumnCount, filterWidth, filterHeight, stride, calculateIndex);
            var ret = new Gpu3DTensor(_cuda, newRows, newColumns, Depth, data.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d.Item1)).ToList());

            List<(object X, object Y)> index = null;
            if(calculateIndex)
                index = data.Select(d => (d.Item2, d.Item3)).ToList();
            return (ret, index);
        }

        public I3DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<(object X, object Y)> indexList)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorReverseMaxPool(_tensorInfo.Value.Single(), RowCount, ColumnCount, rows, columns, indexList);
            return new Gpu3DTensor(_cuda, rows, columns, indexList.Count, ret.Select(d => new GpuMatrix(_cuda, rows, columns, d)).ToList());
        }

        public IMatrix ReverseIm2Col(IReadOnlyList<IReadOnlyList<IVector>> filter, int inputHeight, int inputWidth, int inputDepth, int padding, int filterHeight, int filterWidth, int stride)
        {
            Debug.Assert(IsValid);
            var filters = filter.Select(fl => fl.Cast<GpuVector>().Select(v => v.Memory).ToList()).ToList();
            var ret = _cuda.TensorReverseIm2Col(_tensorInfo.Value, filters, inputHeight, inputWidth, inputDepth, padding, filterHeight, filterWidth, stride);
            return ret.Single().CombineDepthSlices();
        }

        public IMatrix CombineDepthSlices()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.CreateZeroMatrix(_rows, _columns);
            foreach (var item in _data)
                ret.AddInPlace(item);
            return ret;
        }

        public void AddInPlace(I3DTensor tensor)
        {
            var other = (Gpu3DTensor)tensor;
            Debug.Assert(IsValid && other.IsValid);
            for (var i = 0; i < _depth; i++)
                _data[i].AddInPlace(other.GetMatrixAt(i));
        }

        public I3DTensor Multiply(IMatrix matrix)
        {
            var ret = new List<GpuMatrix>();
            foreach (var item in _data)
                ret.Add((GpuMatrix)item.Multiply(matrix));
            var first = ret.First();
            return new Gpu3DTensor(_cuda, first.RowCount, first.ColumnCount, ret.Count, ret);
        }

        public void AddToEachRow(IVector vector)
        {
            foreach (var item in _data)
                item.AddToEachRow(vector);
        }
    }
}
