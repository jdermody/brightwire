using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace BrightWire.Models.Convolutional
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

            public Layer(float[] data, int width, int height, int padding)
            {
                Data = new float[width * height];
                Height = height;
                Width = width;

                int index = 0;
                var sourceWidth = width + padding * 2;
                var sourceHeight = height + padding * 2;
                
                for (var j = 0; j < sourceHeight; j++) {
                    for (var i = 0; i < sourceWidth; i++) {    
                        if (i < padding || j < padding)
                            continue;
                        if (i >= sourceWidth - padding || j >= sourceHeight - padding)
                            continue;
                        Data[index++] = data[j * sourceWidth + i];
                    }
                }
            }

            internal float GetValue(int offset, int padding)
            {
                var width = Width + padding * 2;
                var y = offset / width;
                var x = offset % width;
                if (x < padding || y < padding)
                    return 0;
                else if (x >= width - padding || y >= width - padding)
                    return 0;
                var offset2 = (y - padding) * Width + (x - padding);
                return Data[offset2];
            }

            public float this[int x, int y]
            {
                get
                {
                    return Data[y * Width + x];
                }

                set
                {
                    Data[y * Width + x] = value;
                }
            }
        }

        [ProtoMember(1)]
        public Layer[] Layers { get; set; }

        [ProtoMember(2)]
        public float[] ExpectedOutput { get; set; }

        public Volume AddPadding(int padding)
        {
            return new Volume {
                Layers = Layers.Select(layer => {
                    var width = layer.Width + (padding * 2);
                    var height = layer.Height + (padding * 2);
                    var layerSize = width * height;
                    var data = new float[layerSize];
                    for (var i = 0; i < layerSize; i++)
                        data[i] = layer.GetValue(i, padding);
                    return new Layer(data, width, height);
                }).ToArray()
            };
        }

        public Volume RemovePadding(int padding)
        {
            var ret = new Volume {
                Layers = Layers.Select(l => new Layer(l.Data, l.Width - padding*2, l.Height - padding * 2, padding)).ToArray()
            };
            return ret;
        }

        public IMatrix Im2Col(ILinearAlgebraProvider lap, ConvolutionDescriptor descriptor)
        {
            int xOffset = 0, yOffset = 0;
            var firstLayer = Layers.First();
            int width = firstLayer.Width, height = firstLayer.Height;
            Debug.Assert(Layers.All(l => l.Height == height && l.Width == width));
            int fieldSize = descriptor.FieldSize, stride = descriptor.Stride;

            var data = new List<List<float>>();
            while (yOffset <= height - fieldSize) {
                var column = new List<float>();
                foreach (var layer in Layers) {
                    for (var j = 0; j < fieldSize; j++) {
                        for (var i = 0; i < fieldSize; i++) {
                            column.Add(layer[xOffset + i, yOffset + j]);
                        }
                    }
                }
                data.Add(column);

                // move the window
                xOffset += stride;
                if(xOffset > width - fieldSize) {
                    xOffset = 0;
                    yOffset += stride;
                }
            }
            var firstOutput = data.First();
            return lap.Create(data.Count, firstOutput.Count, (i, j) => data[i][j]);
        }
    }
}
