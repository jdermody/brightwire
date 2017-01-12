using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class Volume
    {
        [ProtoContract]
        public class Layer
        {
            [ProtoMember(1)]
            public int Width { get; set; }

            [ProtoMember(2)]
            public int Height { get; set; }

            [ProtoMember(3)]
            public float[] Data { get; set; }

            public Layer() { }
            public Layer(float[] data, int width, int height)
            {
                Data = data;
                Width = width;
                Height = height;
            }

            public IMatrix AsMatrix(ILinearAlgebraProvider lap)
            {
                return lap.Create(Width, Height, (i, j) => Data[j * Width + i]);
            }
        }

        [ProtoMember(1)]
        public Layer[] Layers { get; set; }

        [ProtoMember(2)]
        public float[] ExpectedOutput { get; set; }

        public I3DTensor AsTensor(ILinearAlgebraProvider lap)
        {
            var matrixList = Layers.Select(l => l.AsMatrix(lap)).ToList();
            var ret = lap.CreateTensor(matrixList);
            foreach (var item in matrixList)
                item.Dispose();
            return ret;
        }
    }
}
