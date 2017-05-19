using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ManagedCuda;
using BrightWire.Models;

namespace BrightWire.LinearAlgebra
{
    internal class Gpu3DTensor : I3DTensor
    {
        readonly IReadOnlyList<GpuMatrix> _data;
        readonly int _rows, _columns, _depth;
        readonly CudaProvider _cuda;

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
            foreach (var item in _data)
                item.Dispose();
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
                return _columns;
            }
        }

        public int Depth
        {
            get
            {
                return _depth;
            }
        }

        public int RowCount
        {
            get
            {
                return _rows;
            }
        }

        public FloatTensor Data
        {
            get
            {
                return new FloatTensor {
                    Matrix = _data.Select(m => m.Data).ToArray()
                };
            }
            set {
                var matrixList = value.Matrix;
                var matrixCount = matrixList.Length;
                for (var i = 0; i < matrixCount && i < _data.Count; i++) {
                    var matrix = matrixList[i];
                    if (matrix.Row != null)
                        _data[i].Data = matrix;
                }
            }
        }

        public IReadOnlyList<IMatrix> DepthSlices => _data;

        public IMatrix GetDepthSlice(int depth)
        {
            return _data[depth];
        }

        public IIndexable3DTensor AsIndexable()
        {
            return _cuda.NumericsProvider
                .CreateTensor(_data.Select(m => _cuda.NumericsProvider.CreateMatrix(m.Data)).ToList())
                .AsIndexable()
            ;
        }

        public IVector ConvertToVector()
        {
            var ret = _cuda.TensorConvertToVector(_data.Select(d => d.CudaDeviceVariable).ToList(), ColumnCount * RowCount);
            return new GpuVector(_cuda, ret.Size, ret);
        }

        public IMatrix ConvertToMatrix()
        {
            var rows = ColumnCount * RowCount;
            var columns = Depth;
            var ret = new CudaDeviceVariable<float>(rows * columns);
            _cuda.TensorConvertToMatrix(_data.Select(d => d.CudaDeviceVariable).ToList(), ColumnCount, RowCount, rows, columns, ret);
            return new GpuMatrix(_cuda, rows, columns, ret);
        }

        public I3DTensor AddPadding(int padding)
        {
            var ret = _cuda.TensorAddPadding(_data.Select(d => d.CudaDeviceVariable).ToList(), RowCount, ColumnCount, padding);
            var newRows = RowCount + padding * 2;
            var newColumns = ColumnCount + padding * 2;
            return new Gpu3DTensor(_cuda, newRows, newColumns, Depth, ret.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d)).ToList());
        }

        public I3DTensor RemovePadding(int padding)
        {
            var ret = _cuda.TensorRemovePadding(_data.Select(d => d.CudaDeviceVariable).ToList(), RowCount, ColumnCount, padding);
            var newRows = RowCount - padding * 2;
            var newColumns = ColumnCount - padding * 2;
            return new Gpu3DTensor(_cuda, newRows, newColumns, Depth, ret.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d)).ToList());
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            var ret = _cuda.TensorIm2Col(_data.Select(d => d.CudaDeviceVariable).ToList(), RowCount, ColumnCount, filterWidth, filterHeight, stride);
            return new GpuMatrix(_cuda, ret.Item2, ret.Item3, ret.Item1);
        }

        public I3DTensor MaxPool(int filterWidth, int filterHeight, int stride, List<Dictionary<Tuple<int, int>, Tuple<int, int>>> indexPosList)
        {
            // TODO: native CUDA implementation
            return _cuda.CreateTensor(AsIndexable().MaxPool(filterWidth, filterHeight, stride, indexPosList).AsIndexable());
        }

        public I3DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<Dictionary<Tuple<int, int>, Tuple<int, int>>> indexPosList)
        {
            // TODO: native CUDA implementation
            return _cuda.CreateTensor(AsIndexable().ReverseMaxPool(rows, columns, indexPosList).AsIndexable());
        }

        public (IMatrix WeightUpdate, IVector BiasUpdate) CalculateWeightUpdate(IMatrix im2Col)
        {
            // TODO: native CUDA implementation
            var ret = AsIndexable().CalculateWeightUpdate(im2Col.AsIndexable());
            return (_cuda.CreateMatrix(ret.WeightUpdate.AsIndexable()), _cuda.CreateVector(ret.BiasUpdate.AsIndexable()));
        }

        public I3DTensor CalculatePreviousError(IMatrix filterMatrix, int inputHeight, int inputWidth, int inputDepth, int padding, int filterHeight, int filterWidth, int stride)
        {
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
