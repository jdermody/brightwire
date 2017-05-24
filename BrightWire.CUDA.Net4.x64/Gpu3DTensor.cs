using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ManagedCuda;
using BrightWire.Models;
using System.Diagnostics;
using System.Threading;

namespace BrightWire.LinearAlgebra
{
    internal class Gpu3DTensor : I3DTensor
    {
        readonly IReadOnlyList<GpuMatrix> _data;
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

        public IMatrix GetDepthSlice(int depth)
        {
            Debug.Assert(IsValid);
            return _data[depth];
        }

        public IIndexable3DTensor AsIndexable()
        {
            Debug.Assert(IsValid);
            return _cuda.NumericsProvider
                .CreateTensor(_data.Select(m => _cuda.NumericsProvider.CreateMatrix(m.Data)).ToList())
                .AsIndexable()
            ;
        }

        public IVector ConvertToVector()
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorConvertToVector(_data.Select(d => d.Memory).ToList(), ColumnCount * RowCount);
            return new GpuVector(_cuda, ret);
        }

        public IMatrix ConvertToMatrix()
        {
            Debug.Assert(IsValid);
            var rows = ColumnCount * RowCount;
            var columns = Depth;
            var ret = _cuda.Allocate(rows * columns);
            _cuda.TensorConvertToMatrix(_data.Select(d => d.Memory).ToList(), ColumnCount, RowCount, rows, columns, ret);
            return new GpuMatrix(_cuda, rows, columns, ret);
        }

        public I3DTensor AddPadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorAddPadding(_data.Select(d => d.Memory).ToList(), RowCount, ColumnCount, padding);
            var newRows = RowCount + padding * 2;
            var newColumns = ColumnCount + padding * 2;
            return new Gpu3DTensor(_cuda, newRows, newColumns, Depth, ret.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d)).ToList());
        }

        public I3DTensor RemovePadding(int padding)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorRemovePadding(_data.Select(d => d.Memory).ToList(), RowCount, ColumnCount, padding);
            var newRows = RowCount - padding * 2;
            var newColumns = ColumnCount - padding * 2;
            return new Gpu3DTensor(_cuda, newRows, newColumns, Depth, ret.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d)).ToList());
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorIm2Col(_data.Select(d => d.Memory).ToList(), RowCount, ColumnCount, filterWidth, filterHeight, stride);
            return new GpuMatrix(_cuda, ret.Item2, ret.Item3, ret.Item1);
        }

        public (I3DTensor Result, IReadOnlyList<(object X, object Y)> Index) MaxPool(int filterWidth, int filterHeight, int stride, bool calculateIndex)
        {
            Debug.Assert(IsValid);
            var newColumns = (ColumnCount - filterWidth) / stride + 1;
            var newRows = (RowCount - filterHeight) / stride + 1;
            var data = _cuda.TensorMaxPool(_data.Select(d => d.Memory).ToList(), RowCount, ColumnCount, filterWidth, filterHeight, stride, calculateIndex);
            var ret = new Gpu3DTensor(_cuda, newRows, newColumns, Depth, data.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d.Item1)).ToList());

            List<(object X, object Y)> index = null;
            if(calculateIndex)
                index = data.Select(d => (d.Item2, d.Item3)).ToList();
            return (ret, index);
        }

        public I3DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<(object X, object Y)> indexList)
        {
            Debug.Assert(IsValid);
            var ret = _cuda.TensorReverseMaxPool(_data.Select(d => d.Memory).ToList(), RowCount, ColumnCount, rows, columns, indexList);
            return new Gpu3DTensor(_cuda, rows, columns, indexList.Count, ret.Select(d => new GpuMatrix(_cuda, rows, columns, d)).ToList());
            // TODO: native CUDA implementation
            //return _cuda.CreateTensor(AsIndexable().ReverseMaxPool(rows, columns, indexList).AsIndexable());
        }

        //public (IMatrix WeightUpdate, IVector BiasUpdate) CalculateWeightUpdate(IMatrix im2Col)
        //{
        //    Debug.Assert(IsValid && im2Col.IsValid);
        //    var multiplyWith = ConvertToMatrix();
        //    var weightUpdate = im2Col.TransposeThisAndMultiply(multiplyWith);
        //    var biasUpdate = multiplyWith.ColumnSums();
        //    biasUpdate.Multiply(1f / multiplyWith.RowCount);
        //    return (weightUpdate, biasUpdate);
        //    // TODO: native CUDA implementation
        //    //var ret = AsIndexable().CalculateWeightUpdate(im2Col.AsIndexable());
        //    //return (_cuda.CreateMatrix(ret.WeightUpdate.AsIndexable()), _cuda.CreateVector(ret.BiasUpdate.AsIndexable()));
        //}

        public I3DTensor CalculatePreviousError(IMatrix filterMatrix, int inputHeight, int inputWidth, int inputDepth, int padding, int filterHeight, int filterWidth, int stride)
        {
            Debug.Assert(IsValid && filterMatrix.IsValid);
            // TODO: native CUDA implementation
            return _cuda.CreateTensor(AsIndexable().CalculatePreviousError(
                filterMatrix,
                inputHeight,
                inputWidth,
                inputDepth,
                padding,
                filterHeight,
                filterWidth,
                stride
            ).AsIndexable());
        }

        public IMatrix ReverseIm2Col(IReadOnlyList<IReadOnlyList<IVector>> filter, int inputHeight, int inputWidth, int inputDepth, int padding, int filterHeight, int filterWidth, int stride)
        {
            var columns = inputHeight + padding * 2;
            var rows = inputWidth + padding * 2;
            var filters = filter.Select(fl => fl.Cast<GpuVector>().Select(v => v.Memory).ToList()).ToList();
            var matrixList = _cuda.TensorReverseIm2Col(_data.Select(d => d.Memory).ToList(), filters, RowCount, ColumnCount, inputHeight, inputWidth, inputDepth, padding, filterHeight, filterWidth, stride);
            var matrixList2 = matrixList.Select(d => new GpuMatrix(_cuda, rows * columns, inputDepth, d)).ToList();

            var ret = _cuda.CreateMatrix(columns * rows, inputDepth);
            foreach (var item in matrixList2)
                ret = ret.Add(item);
            //var ret = matrixList2.First();
            //foreach(var item in matrixList2.Skip(1)) {
            //    ret.AddInPlace(item);
            //    item.Dispose();
            //}
            return ret;
        }
    }
}
