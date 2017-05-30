using BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.CUDA.Helper
{
    class TensorOutput : IDisposable
    {
        readonly CudaProvider _cuda;
        readonly IMatrix _data;
        readonly int _rows, _columns, _depth, _count;
        readonly List<IDeviceMemoryPtr[]> _ptr = new List<IDeviceMemoryPtr[]>();

        public TensorOutput(CudaProvider cuda, int rows, int columns, int depth, int count, bool setToZero)
        {
            _cuda = cuda;
            _rows = rows;
            _columns = columns;
            _depth = depth;
            _count = count;

            // allocate space for the output
            _data = setToZero
                ? cuda.CreateZeroMatrix(rows * columns * depth, count)
                : cuda.CreateMatrix(rows * columns * depth, count)
            ;   

            // get the pointers
            for(var i = 0; i < count; i++) {
                var tensor = _data.Column(i).Split(depth).Cast<GpuVector>().Select(v => v.Memory).ToArray();
                _ptr.Add(tensor);
            }
        }

        public void Dispose()
        {
            _data.Dispose();
        }

        public int Rows => _rows;
        public int Columns => _columns;
        public int Depth => _depth;
        public int Count => _count;

        internal DeviceMemoryPtrList GetDeviceMemoryPtr() => new DeviceMemoryPtrList(_ptr);

        public I3DTensor Single()
        {
            Debug.Assert(_count == 1);
            return GetAt(0);
        }
        public I3DTensor GetAt(int index)
        {
            var matrixList = _data.Column(index)
                .Split(_depth)
                .Select(v => v.ConvertInPlaceToMatrix(_rows, _columns))
                .Cast<GpuMatrix>()
                .ToList()
            ;
            return new Gpu3DTensor(_cuda, _rows, _columns, _depth, matrixList);
        }
        public I4DTensor GetAsTensor()
        {
            return new Gpu4DTensor(_cuda, _data, _rows, _columns, _depth);
        }
    }
}
