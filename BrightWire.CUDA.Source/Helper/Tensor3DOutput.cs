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
        readonly List<IMatrix> _data = new List<IMatrix>();
	    readonly CUdeviceptr[] _ptr;
		readonly CudaProvider _cuda;

        public Tensor3DOutput(CudaProvider cuda, int rows, int columns, int depth, bool setToZero)
        {
	        _cuda = cuda;
            Rows = rows;
            Columns = columns;

            for (var i = 0; i < depth; i++) {
                _data.Add(setToZero
                    ? cuda.CreateZeroMatrix(rows, columns)
                    : cuda.CreateMatrix(rows, columns)
                );
            }
            _ptr = _data.Cast<GpuMatrix>().Select(m => m.Memory.DevicePointer).ToArray();
        }

        public void Release()
        {
            foreach(var item in _data)
                item.Dispose();
        }

        public int Rows { get; }

	    public int Columns { get; }

	    public int Count => _data.Count;

        internal CudaDeviceVariable<CUdeviceptr> GetDeviceMemoryPtr()
        {
            var ret = new CudaDeviceVariable<CUdeviceptr>(Count);
            ret.CopyToDevice(_ptr);
            return ret;
        }

        public IMatrix Single()
        {
            return _data.Single();
        }

        public I3DTensor GetAsTensor()
        {
	        return _cuda.Create3DTensor(_data);
        }

	    public IMatrix GetAsMatrix()
	    {
		    if (_data.Count == 1)
			    return _data[0];

		    using (var tensor = GetAsTensor()) {
			    var ret = tensor.CombineDepthSlices();
				// TODO: divide by the number of depth slices?
				return ret;
		    }
	    }
    }
}
