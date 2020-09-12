using System;
using System.Collections.Generic;

namespace BrightData.Cuda.Helper
{
    class ConvolutionsData : IDisposable
    {
		public ConvolutionsData(CudaProvider cuda, List<(uint X, uint Y)> convolutions)
	    {
		    Count = (uint)convolutions.Count;
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
		public uint Count { get; }

		public void Dispose()
	    {
		    X.Free();
		    Y.Free();
	    }
    }
}
