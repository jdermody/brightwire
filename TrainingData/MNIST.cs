using BrightWire.Helper;
using BrightWire.Models.Convolutional;
using BrightWire.Models.Input;
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
        /// Input layer size
        /// </summary>
        public const int INPUT_SIZE = 784;

        /// <summary>
        /// Output layer size
        /// </summary>
        public const int OUTPUT_SIZE = 10;

        /// <summary>
        /// Image data
        /// </summary>
        public class Image
        {
            readonly byte[] _data;
            readonly int _label;

            internal Image(byte[] data, int label)
            {
                _data = data;
                _label = label;
            }

            /// <summary>
            /// The image data
            /// </summary>
            public byte[] Data { get { return _data; } }

            /// <summary>
            /// The image number (0-9)
            /// </summary>
            public int Label { get { return _label; } }

            /// <summary>
            /// Converts the image to one hot encoded float arrays
            /// </summary>
            public TrainingExample Sample
            {
                get
                {
                    var data = _data.Select(b => Convert.ToSingle((int)b) / 255f).ToArray();
                    var label = new float[10];
                    label[_label] = 1;
                    return new TrainingExample(data, label);
                }
            }

            public Volume Convolutional
            {
                get
                {
                    var label = new float[10];
                    label[_label] = 1;
                    var data = _data.Select(b => Convert.ToSingle((int)b) / 255f).ToArray();
                    return new Volume {
                        ExpectedOutput = label,
                        Layers = new[] {
                            new Volume.Layer {
                                Data = data,
                                Width = 28,
                                Height = 28
                            }
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Loads a set of images from the MNIST data files
        /// </summary>
        /// <param name="labelPath">Path to the label data file</param>
        /// <param name="imagePath">Path to the image data file</param>
        public static IReadOnlyList<Image> Load(string labelPath, string imagePath)
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
