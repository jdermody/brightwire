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
		public ConvolutionsData(CudaProvider cuda, List<(int X, int Y)> convolutions)
	    {
		    Count = convolutions.Count;
		    X = cuda.Allocate(Count);
		    Y = cuda.Allocate(Count);

		    var xData = new float[Count];
		    var yData = new float[Count];
		    for (var i = 0; i < Count; i++) {
			    var item = convolutions[i];
			    xData[i] = item.X;
			    yData[i] = item.Y;
		    }

		    X.CopyToDevice(xData);
		    Y.CopyToDevice(yData);
	    }

		public IDeviceMemoryPtr X { get; }
		public IDeviceMemoryPtr Y { get; }
		public int Count { get; }

		public void Dispose()
	    {
		    X.Free();
		    Y.Free();
	    }
    }
}
