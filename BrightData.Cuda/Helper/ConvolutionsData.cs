using System;
using System.Collections.Generic;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.Cuda.Helper
{
    internal unsafe class ConvolutionsData : IDisposable
    {
		public ConvolutionsData(CudaProvider cuda, List<(uint X, uint Y)> convolutions)
	    {
		    Count = (uint)convolutions.Count;
		    X = cuda.Allocate(Count);
		    Y = cuda.Allocate(Count);

		    using var xData = SpanOwner<float>.Allocate((int)Count);
            using var yData = SpanOwner<float>.Allocate((int)Count);
            var xDataArray = xData.DangerousGetArray().Array!;
            var yDataArray = yData.DangerousGetArray().Array!;
		    for (var i = 0; i < Count; i++) {
			    var item = convolutions[i];
                xDataArray[i] = item.X;
                yDataArray[i] = item.Y;
		    }

		    X.CopyToDevice(xDataArray);
		    Y.CopyToDevice(yDataArray);
	    }

		public IDeviceMemoryPtr X { get; }
		public IDeviceMemoryPtr Y { get; }
		public uint Count { get; }

		public void Dispose()
	    {
		    X.Release();
		    Y.Release();
	    }
    }
}
