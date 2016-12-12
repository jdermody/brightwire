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

        public int InputSize { get; set; }

        public int InputDepth { get; set; }

        public int FieldSize { get; set; }

        public int Stride { get; set; }

        public int FilterDepth { get; set; }

        public int Padding { get; set; }

        public int LocationCount
        {
            get
            {
                return (InputSize - FieldSize + Padding*2) / Stride + 1;
            }
        }

        public int Extent
        {
            get
            {
                return FieldSize * FieldSize * InputDepth;
            }
        }
    }
}
