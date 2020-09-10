using BrightWire.Models;
using BrightWire.TrainingData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;

namespace BrightWire.TrainingData.WellKnown
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
            public byte[] Data => _data;

            /// <summary>
            /// The image number (0-9)
            /// </summary>
            public int Label => _label;

            /// <summary>
            /// Converts the image to one hot encoded float arrays
            /// </summary>
            public (Vector<float> Data, Vector<float> Label) AsFloatArray
            {
                get {
                    var label = new float[10];
                    label[_label] = 1;

                    return (
                        FloatVector.Create(_data.Select(b => Convert.ToSingle((int)b) / 255f).ToArray()),
                        FloatVector.Create(label)
                    );
                }
            }

            /// <summary>
            /// Converts the image to a tensor with one hot encoded label vector
            /// </summary>
            public (Tensor3D<float> Tensor, Vector<float> Label) AsFloatTensor
            {
                get {
                    const int SIZE = 28;
                    var data = AsFloatArray;
                    var rows = new List<Vector<float>>();
                    var vector = data.Data;

                    for (var y = 0; y < SIZE; y++) {
                        var row = new float[SIZE];
                        for (var x = 0; x < SIZE; x++)
                            row[x] = vector[(y * SIZE) + x];
                        rows.Add(FloatVector.Create(row));
                    }
                    var tensor = Float3DTensor.Create(new[] { 
                        FloatMatrix.Create(rows.ToArray()) 
                    });
                    return (tensor, data.Label);
                }
    }
}

/// <summary>
/// Loads a set of images from the MNIST data files
/// </summary>
/// <param name="labelPath">Path to the label data file</param>
/// <param name="imagePath">Path to the image data file</param>
/// <param name="total">Maximum number of images to load</param>
public static IReadOnlyList<Image> Load(string labelPath, string imagePath, int total = int.MaxValue)
{
    var labels = new List<byte>();
    using (var file = new FileStream(labelPath, FileMode.Open, FileAccess.Read))
    using (var reader = new BigEndianBinaryReader(file)) {
        reader.ReadInt32();
        var count = reader.ReadUInt32();
        for (var i = 0; i < count && i < total; i++) {
            labels.Add(reader.ReadByte());
        }
    }

    var images = new List<byte[]>();
    using (var file = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
    using (var reader = new BigEndianBinaryReader(file)) {
        reader.ReadInt32();
        var count = reader.ReadUInt32();
        var numRows = reader.ReadUInt32();
        var numCols = reader.ReadUInt32();
        var imageSize = numRows * numCols;
        for (var i = 0; i < count && i < total; i++) {
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
