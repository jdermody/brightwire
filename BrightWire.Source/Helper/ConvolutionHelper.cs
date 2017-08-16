using System.Collections.Generic;

namespace BrightWire.Helper
{
    /// <summary>
    /// Helper class to calculate convolutional indices
    /// </summary>
    static class ConvolutionHelper
    {
        public static List<(int X, int Y)[]> LeftToRight(int width, int height, int filterWidth, int filterHeight, int stride)
        {
            int y = 0, x = 0;
            var ret = new List<(int X, int Y)[]>();

            if (x <= width - filterWidth) {
                while (y <= height - filterHeight) {
                    var filter = new(int X, int Y)[filterWidth * filterHeight];
                    var index = 0;
                    for (var j = 0; j < filterHeight; j++) {
                        for (var i = 0; i < filterWidth; i++)
                            filter[index++] = (x + i, y + j);
                    }
                    ret.Add(filter);

                    // move the window
                    x += stride;
                    if (x > width - filterWidth) {
                        x = 0;
                        y += stride;
                    }
                }
            }
            return ret;
        }

        public static List<(int X, int Y)[]> TopToBottom(int width, int height, int filterWidth, int filterHeight, int stride)
        {
            int y = 0, x = 0;
            var ret = new List<(int X, int Y)[]>();

            if (y <= height - filterHeight) {
                while (x <= width - filterWidth) {
                    var filter = new(int X, int Y)[filterWidth * filterHeight];
                    var index = 0;
                    for (var i = 0; i < filterWidth; i++) {
                        for (var j = 0; j < filterHeight; j++)
                            filter[index++] = (x + i, y + j);
                    }
                    ret.Add(filter);

                    // move the window
                    y += stride;
                    if (y > height - filterHeight) {
                        y = 0;
                        x += stride;
                    }
                }
            }
            return ret;
        }

        public delegate List<(int X, int Y)[]> ConvolutionalDelegate(int width, int height, int filterWidth, int filterHeight, int stride);
        public static ConvolutionalDelegate Default = TopToBottom;
    }
}
