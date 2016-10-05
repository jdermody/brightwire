using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TrainingData
{
    /// <summary>
    /// Helper class for MNIST data: http://yann.lecun.com/exdb/mnist/
    /// </summary>
    public static class Mnist
    {
        /// <summary>
        /// Image data
        /// </summary>
        public class Image
        {
            readonly byte[] _data;
            readonly int _label;

            public Image(byte[] data, int label)
            {
                _data = data;
                _label = label;
            }

            public byte[] Data { get { return _data; } }
            public int Label { get { return _label; } }

            public Tuple<IVector, IIndexableVector, IIndexableVector> AsSample(ILinearAlgebraProvider provider)
            {
                var data = provider.Create(_data.Select(b => Convert.ToSingle((int)b) / 255));
                var label = provider.Create(10, 0.0f).AsIndexable();
                label[_label] = 1;
                return Tuple.Create(data, data.AsIndexable(), label);
            }

            public Tuple<float[], float[]> Sample
            {
                get
                {
                    var data = _data.Select(b => Convert.ToSingle((int)b) / 255).ToArray();
                    var label = new float[10];
                    label[_label] = 1;
                    return Tuple.Create(data, label);
                }
            }
        }

        public static IEnumerable<Image> Load(string labelPath, string imagePath)
        {
            var labels = new List<byte>();
            using(var file = new FileStream(labelPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BigEndianBinaryReader(file)) {
                reader.ReadInt32();
                var count = reader.ReadUInt32();
                for (var i = 0; i < count; i++) {
                    labels.Add(reader.ReadByte());
                }
            }

            var images = new List<byte[]>();
            using(var file = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BigEndianBinaryReader(file)) {
                reader.ReadInt32();
                var count = reader.ReadUInt32();
                var numRows = reader.ReadUInt32();
                var numCols = reader.ReadUInt32();
                var imageSize = numRows * numCols;
                for (var i = 0; i < count; i++) {
                    var imageData = new byte[imageSize];
                    for (var j = 0; j < imageSize; j++) {
                        imageData[j] = reader.ReadByte();
                    }
                    images.Add(imageData);
                }
            }

            return labels.Zip(images, (l, d) => new Image(d, l)).ToList();
        }
    }
}
