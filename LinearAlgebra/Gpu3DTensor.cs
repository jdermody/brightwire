using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public IVector ConvertInPlaceToVector()
        {
            var ret = _cuda.TensorConvertToVector(_data.Select(d => d.CudaDeviceVariable).ToList(), ColumnCount * RowCount);
            return new GpuVector(_cuda, ret.Size, ret);
        }

        public I3DTensor AddPadding(int padding)
        {
            var ret = _cuda.TensorAddPadding(_data.Select(d => d.CudaDeviceVariable).ToList(), RowCount, ColumnCount, padding);
            var newRows = RowCount + padding * 2;
            var newColumns = ColumnCount + padding * 2;
            return new Gpu3DTensor(_cuda, newRows, newColumns, Depth, ret.Select(d => new GpuMatrix(_cuda, newRows, newColumns, d)).ToList());
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            var ret = _cuda.TensorIm2Col(_data.Select(d => d.CudaDeviceVariable).ToList(), RowCount, ColumnCount, filterWidth, filterHeight, stride);
            return new GpuMatrix(_cuda, ret.Item2, ret.Item3, ret.Item1);
        }

        public I3DTensor RemovePadding(int padding)
        {
            throw new NotImplementedException();
        }
    }
}
