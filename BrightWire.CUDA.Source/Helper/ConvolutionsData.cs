using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightWire.Cuda.Helper;
using BrightWire.LinearAlgebra;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightWire.CUDA.Source.Helper
{
    class ConvolutionsData : IDisposable
    {
	    readonly IDeviceMemoryPtr _x, _y;
	    readonly int _count;

	    public ConvolutionsData(CudaProvider cuda, List<(int X, int Y)> convolutions)
	    {
		    _count = convolutions.Count;
		    _x = cuda.Allocate(_count);
		    _y = cuda.Allocate(_count);

		    var xData = new float[_count];
		    var yData = new float[_count];
		    for (var i = 0; i < _count; i++) {
			    var item = convolutions[i];
			    xData[i] = item.X;
			    yData[i] = item.Y;
		    }

		    _x.CopyToDevice(xData);
		    _y.CopyToDevice(yData);
	    }

		public IDeviceMemoryPtr X => _x;
	    public IDeviceMemoryPtr Y => _y;
	    public int Count => _count;

	    public void Dispose()
	    {
		    _x.Free();
		    _y.Free();
	    }
    }
}
