using BrightWire.Connectionist;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Convolutional
{
    public class ConvolutionDescriptor : LayerDescriptor
    {
        public ConvolutionDescriptor(float regularisation) : base(regularisation)
        {

        }


        public int Padding { get; set; }
        public int Stride { get; set; }
        public int FilterWidth { get; set; }
        public int FilterHeight { get; set; }
        public int FilterDepth { get; set; }

        public int FilterSize
        {
            get
            {
                return FilterWidth * FilterHeight;
            }
        }

        public Tuple<int, int> CalculateExtent(int inputWidth, int inputHeight)
        {
            var padding = 2 * Padding;
            return Tuple.Create((inputWidth - FilterWidth + padding) / Stride + 1, (inputHeight - FilterHeight + padding) / Stride + 1);
        }
    }
}
