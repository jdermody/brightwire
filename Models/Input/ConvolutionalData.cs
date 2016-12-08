using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace BrightWire.Models.Input
{
    [ProtoContract]
    public class ConvolutionalData
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

            public float GetValue(int offset, int padding)
            {
                var y = offset / Width;
                var x = offset % Width;
                if (x <= padding || y <= padding)
                    return 0;
                else if (x >= Width - padding || y >= Height - padding)
                    return 0;
                var offset2 = (y - padding) * Width + (x - padding);
                return Data[offset2];
            }

            public static Layer Create(float[] data, int width, int height, int padding)
            {
                int index = 0;
                var ret = new Layer {
                    Data = new float[width * height],
                    Height = height,
                    Width = width
                };
                for (var j = 0; j < height + padding * 2; j++) {
                    for (var i = 0; i < width + padding*2; i++) {    
                        if (i <= padding || j <= padding)
                            continue;
                        if (i >= width - padding || j >= height - padding)
                            continue;
                        ret.Data[index++] = data[j * width + i];
                    }
                }
                return ret;
            }
        }

        [ProtoMember(1)]
        public Layer[] Layers { get; set; }

        public IVector CreateVector(ILinearAlgebraProvider lap, int padding)
        {
            var firstLayer = Layers.First();
            Debug.Assert(Layers.All(l => l.Data.Length == l.Width * l.Height && l.Width == firstLayer.Width && l.Height == firstLayer.Height));

            var numLayers = Layers.Length;
            var width = firstLayer.Width + (padding * 2);
            var height = firstLayer.Height + (padding * 2);
            var layerSize = width * height;
            var totalSize = layerSize * numLayers;

            return lap.Create(totalSize, i => {
                var layerIndex = i / layerSize;
                var layerOffset = i % layerSize;
                return Layers[layerIndex].GetValue(layerOffset, padding);
            });
        }

        public static ConvolutionalData FromVector(IVector vector, int layers, int width, int height, int padding)
        {
            var layerSize = vector.Count / layers;
            var ret = new ConvolutionalData {
                Layers = new Layer[layerSize]
            };
            for(var i = 0; i < layerSize; i++) {
                var indexList = Enumerable.Range(i * layerSize, layerSize).ToArray();
                using (var data = vector.GetNewVectorFromIndexes(indexList))
                    ret.Layers[i] = Layer.Create(data.Data.Data, width, height, padding);
            }
            return ret;
        }
    }
}
