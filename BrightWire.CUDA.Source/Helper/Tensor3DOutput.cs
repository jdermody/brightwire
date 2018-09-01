using BrightWire.LinearAlgebra;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Cuda.Helper
{
    /// <summary>
    /// Helper class to represent a list of matrices that are the output of a cuda kernel
    /// </summary>
    class Tensor3DOutput
    {
	    readonly IMatrix _data;
	    readonly CUdeviceptr[] _ptr;

        public Tensor3DOutput(CudaProvider cuda, int rows, int columns, int depth, bool setToZero)
        {
            Rows = rows;
            Columns = columns;
	        Depth = depth;

	        _data = setToZero
		        ? cuda.CreateZeroMatrix(rows * columns, depth)
		        : cuda.CreateMatrix(rows * columns, depth);

	        _ptr = Enumerable.Range(0, depth).Select(i => ((GpuVector) _data.Column(i)).Memory.DevicePointer).ToArray();
        }

        public int Rows { get; }

	    public int Columns { get; }

	    public int Depth { get; }

        internal CudaDeviceVariable<CUdeviceptr> GetDeviceMemoryPtr()
        {
            var ret = new CudaDeviceVariable<CUdeviceptr>(_ptr.Length);
            ret.CopyToDevice(_ptr);
            return ret;
        }

        public I3DTensor GetAsTensor()
        {
	        return _data.ConvertTo3DTensor(Rows, Columns);
        }

	    public IMatrix GetAsMatrix()
	    {
		    if (Depth == 1)
			    return _data.ConvertInPlaceToVector().ConvertInPlaceToMatrix(Rows, Columns);

		    using (var tensor = GetAsTensor()) {
			    var ret = tensor.CombineDepthSlices();
				// TODO: divide by the number of depth slices?
				return ret;
		    }
	    }
    }
}
