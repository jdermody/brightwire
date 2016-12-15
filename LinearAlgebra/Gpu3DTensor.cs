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
            throw new NotImplementedException();
        }

        public I3DTensor AddPadding(int padding)
        {
            throw new NotImplementedException();
        }

        public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
        {
            throw new NotImplementedException();
        }

        public I3DTensor RemovePadding(int padding)
        {
            throw new NotImplementedException();
        }
    }
}
