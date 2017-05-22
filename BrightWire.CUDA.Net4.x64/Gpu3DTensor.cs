using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ManagedCuda;
using BrightWire.Models;
using System.Diagnostics;

namespace BrightWire.LinearAlgebra
{
    internal class Gpu3DTensor : I3DTensor
    {
        readonly IReadOnlyList<GpuMatrix> _data;
        readonly int _rows, _columns, _depth;
        readonly CudaProvider _cuda;
        bool _disposed = false;

#if DEBUG
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
        }

        ~Gpu3DTensor()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed) {
                _disposed = true;
                foreach (var item in _data)
                    item.Memory.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        public (I3DTensor Result, IReadOnlyList<(int[] X, int[] Y)> Index) MaxPool(int filterWidth, int filterHeight, int stride)
        {
            Debug.Assert(IsValid);
            var newColumns = (ColumnCount - filterWidth) / stride + 1;
            var newRows = (RowCount - filterHeight) / stride + 1;
            var data = _cuda.TensorMaxPool(_data.Select(d => d.Memory).ToList(), RowCount, ColumnCount, filterWidth, filterHeight, stride);
            var ret = new Gpu3DTensor(_cuda, newRows, newColumns, Depth, data.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d.Item1)).ToList());
            var index = data.Select(d => (d.Item2, d.Item3)).ToList();
            return (ret, index);
        }

        public I3DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<(int[] X, int[] Y)> indexList)
        {
            Debug.Assert(IsValid);
            // TODO: native CUDA implementation
            return _cuda.CreateTensor(AsIndexable().ReverseMaxPool(rows, columns, indexList).AsIndexable());
        }

        public (IMatrix WeightUpdate, IVector BiasUpdate) CalculateWeightUpdate(IMatrix im2Col)
        {
            Debug.Assert(IsValid && im2Col.IsValid);

            // TODO: native CUDA implementation
            var ret = AsIndexable().CalculateWeightUpdate(im2Col.AsIndexable());
            return (_cuda.CreateMatrix(ret.WeightUpdate.AsIndexable()), _cuda.CreateVector(ret.BiasUpdate.AsIndexable()));
        }

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
    }
}
