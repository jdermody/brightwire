using BrightWire.Connectionist;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Connectionist
{
    /// <summary>
    /// Describes a convolutional operation
    /// </summary>
    public class ConvolutionDescriptor : LayerDescriptor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="regularisation">Regularisation parameter</param>
        public ConvolutionDescriptor(float regularisation) : base(regularisation)
        {

        }

        /// <summary>
        /// The padding to apply to the input before the convolution
        /// </summary>
        public int Padding { get; set; }

        /// <summary>
        /// The stride of the convolution - the increment that each subsequent convolution moves from the previous
        /// </summary>
        public int Stride { get; set; }

        /// <summary>
        /// The width of the convolution filter
        /// </summary>
        public int FilterWidth { get; set; }

        /// <summary>
        /// The height of the convolution filter
        /// </summary>
        public int FilterHeight { get; set; }

        /// <summary>
        /// The depth of the convolution filter
        /// </summary>
        public int FilterDepth { get; set; }

        /// <summary>
        /// The filter width times height
        /// </summary>
        public int FilterSize
        {
            get
            {
                return FilterWidth * FilterHeight;
            }
        }

        /// <summary>
        /// Calculates the size of the output after the convolution has completed
        /// </summary>
        /// <param name="inputWidth">The width of the input image</param>
        /// <param name="inputHeight">The height of the input image</param>
        /// <returns></returns>
        public Tuple<int, int> CalculateExtent(int inputWidth, int inputHeight)
        {
            var padding = 2 * Padding;
            return Tuple.Create((inputWidth + padding - FilterWidth) / Stride + 1, (inputHeight + padding - FilterHeight) / Stride + 1);
        }

        /// <summary>
        /// Clones the current convolution descriptor
        /// </summary>
        /// <returns>A copy of the current descriptor</returns>
        public new ConvolutionDescriptor Clone()
        {
            var ret = new ConvolutionDescriptor(Lambda);
            ret.Padding = Padding;
            ret.Stride = Stride;
            ret.FilterWidth = FilterWidth;
            ret.FilterHeight = FilterHeight;
            ret.FilterDepth = FilterDepth;
            return ret;
        }
    }
}
