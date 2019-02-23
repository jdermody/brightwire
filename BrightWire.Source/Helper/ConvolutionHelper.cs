using System.Collections.Generic;

namespace BrightWire.Helper
{
    /// <summary>
    /// Helper class to calculate convolutional indices
    /// </summary>
    public static class ConvolutionHelper
    {
	    static readonly Dictionary<(int, int, int, int, int, int), List<(int X, int Y)>> _leftToRight = new Dictionary<(int, int, int, int, int, int), List<(int, int)>>();
	    static readonly Dictionary<(int, int, int, int, int, int), List<(int X, int Y)>> _topToBottm = new Dictionary<(int, int, int, int, int, int), List<(int, int)>>();
		    
		/// <summary>
		/// Generates convolution indices from left to right
		/// </summary>
		/// <param name="width">Input width</param>
		/// <param name="height">Input height</param>
		/// <param name="filterWidth">Filter width</param>
		/// <param name="filterHeight">Filter height</param>
		/// <param name="xStride">X Stride</param>
		/// <param name="yStride">Y Stride</param>
		/// <returns>List of (x, y) indices</returns>
        public static List<(int X, int Y)> LeftToRight(int width, int height, int filterWidth, int filterHeight, int xStride, int yStride)
		{
			var key = (width, height, filterWidth, filterHeight, xStride, yStride);
			if (_leftToRight.TryGetValue(key, out var ret))
				return ret;

            int y = 0, x = 0;
            ret = new List<(int X, int Y)>();

            if (x <= width - filterWidth) {
                while (y <= height - filterHeight) {
	                ret.Add((x, y));

                    // move the window
                    x += xStride;
                    if (x > width - filterWidth) {
                        x = 0;
                        y += yStride;
                    }
                }
            }

			_leftToRight[key] = ret;
            return ret;
        }

	    /// <summary>
	    /// Generates convolution indices from top to bottom
	    /// </summary>
	    /// <param name="width">Input width</param>
	    /// <param name="height">Input height</param>
	    /// <param name="filterWidth">Filter width</param>
	    /// <param name="filterHeight">Filter height</param>
	    /// <param name="xStride">X Stride</param>
	    /// <param name="yStride">Y Stride</param>
	    /// <returns>List of (x, y) indices</returns>
        public static List<(int X, int Y)> TopToBottom(int width, int height, int filterWidth, int filterHeight, int xStride, int yStride)
        {
	        var key = (width, height, filterWidth, filterHeight, xStride, yStride);
	        if (_topToBottm.TryGetValue(key, out var ret))
		        return ret;

            int y = 0, x = 0;
            ret = new List<(int X, int Y)>();

            if (y <= height - filterHeight) {
                while (x <= width - filterWidth) {
	                ret.Add((x, y));

                    // move the window
                    y += xStride;
                    if (y > height - filterHeight) {
                        y = 0;
                        x += yStride;
                    }
                }
            }

	        _topToBottm[key] = ret;
            return ret;
        }

	    /// <inheritdoc />
	    public delegate List<(int X, int Y)> ConvolutionalDelegate(int width, int height, int filterWidth, int filterHeight, int xStride, int yStride);

		/// <summary>
		/// Default convolutional direction
		/// </summary>
        public static ConvolutionalDelegate Default = LeftToRight;
    }
}
