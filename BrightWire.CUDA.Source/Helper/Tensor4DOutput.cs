using BrightWire.LinearAlgebra;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BrightWire.Cuda.Helper
{
    /// <summary>
    /// Helper class that allocates space for a 4D tensor output from a cuda kernel
    /// </summary>
    class Tensor4DOutput
    {
        readonly CudaProvider _cuda;
        readonly IMatrix _data;
	    readonly List<IDeviceMemoryPtr[]> _ptr = new List<IDeviceMemoryPtr[]>();

        public Tensor4DOutput(CudaProvider cuda, int rows, int columns, int depth, int count, bool setToZero)
        {
            _cuda = cuda;
            Rows = rows;
            Columns = columns;
            Depth = depth;
            Count = count;

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

        public void Release()
        {
            _data.Dispose();
        }

        public int Rows { get; }
	    public int Columns { get; }
	    public int Depth { get; }
	    public int Count { get; }

	    internal DeviceMemoryPtrToPtrList GetDeviceMemoryPtr() => new DeviceMemoryPtrToPtrList(_ptr);

        public I3DTensor Single()
        {
            Debug.Assert(Count == 1);
            return GetAt(0);
        }
        public I3DTensor GetAt(int index)
        {
            var matrixList = _data.Column(index)
                .Split(Depth)
                .Select(v => v.AsMatrix(Rows, Columns))
                .Cast<GpuMatrix>()
                .ToList()
            ;
            return _cuda.Create3DTensor(matrixList);
        }
    }
}
