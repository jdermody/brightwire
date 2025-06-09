using System.Collections.Generic;

namespace BrightData.Helper
{
	/// <summary>
	/// Helper class to calculate convolutional indices
	/// </summary>
	public static class ConvolutionHelper
	{
		static readonly Dictionary<(uint, uint, uint, uint, uint, uint), List<(uint X, uint Y)>> LeftToRightCache = [];
		static readonly Dictionary<(uint, uint, uint, uint, uint, uint), List<(uint X, uint Y)>> TopToBottomCache = [];

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
		public static List<(uint X, uint Y)> LeftToRight(uint width, uint height, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
		{
			var key = (width, height, filterWidth, filterHeight, xStride, yStride);
			if (LeftToRightCache.TryGetValue(key, out var ret))
				return ret;

			uint y = 0, x = 0;
			ret = [];

            while (y <= height - filterHeight) {
                ret.Add((x, y));

                // move the window
                x += xStride;
                if (x > width - filterWidth) {
                    x = 0;
                    y += yStride;
                }
            }

            LeftToRightCache[key] = ret;
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
		public static List<(uint X, uint Y)> TopToBottom(uint width, uint height, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
		{
			var key = (width, height, filterWidth, filterHeight, xStride, yStride);
			if (TopToBottomCache.TryGetValue(key, out var ret))
				return ret;

			uint y = 0, x = 0;
			ret = [];

            while (x <= width - filterWidth) {
                ret.Add((x, y));

                // move the window
                y += xStride;
                if (y > height - filterHeight) {
                    y = 0;
                    x += yStride;
                }
            }

			TopToBottomCache[key] = ret;
			return ret;
		}

		/// <inheritdoc />
		public delegate List<(uint X, uint Y)> ConvolutionalDelegate(uint width, uint height, uint filterWidth, uint filterHeight, uint xStride, uint yStride);

		/// <summary>
		/// Default convolutional direction
		/// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly ConvolutionalDelegate Default = LeftToRight;
	}
}
