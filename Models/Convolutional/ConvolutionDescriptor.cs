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

        public int FieldSize { get; set; }

        public int Stride { get; set; }

        public int FilterDepth { get; set; }

        public int Extent { get { return FieldSize * FieldSize * Stride; } }
    }
}
