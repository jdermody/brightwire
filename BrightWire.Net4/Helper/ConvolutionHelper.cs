using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Helper
{
    static class ConvolutionHelper
    {
        public static IReadOnlyList<IReadOnlyList<(int X, int Y)>> LeftToRight(int width, int height, int filterWidth, int filterHeight, int stride)
        {
            int y = 0, x = 0;
            var ret = new List<IReadOnlyList<(int X, int Y)>>();

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

        public static IReadOnlyList<IReadOnlyList<(int X, int Y)>> TopToBottom(int width, int height, int filterWidth, int filterHeight, int stride)
        {
            int y = 0, x = 0;
            var ret = new List<IReadOnlyList<(int X, int Y)>>();

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

        public delegate IReadOnlyList<IReadOnlyList<(int X, int Y)>> ConvolutionalDelegate(int width, int height, int filterWidth, int filterHeight, int stride);
        public static ConvolutionalDelegate Default = TopToBottom;
    }
}
