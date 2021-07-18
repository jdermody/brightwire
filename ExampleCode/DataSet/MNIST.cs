using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.Models;
using BrightWire.TrainingData.Helper;
using ExampleCode.DataTableTrainers;

namespace ExampleCode.DataSet
{
    internal class Mnist
    {
        readonly IBrightDataContext _context;
        public Image[] TrainingImages { get; }
        public Image[] TestImages { get; }

        public Mnist(IBrightDataContext context, Image[] trainingImages, Image[] testImages)
        {
            _context = context;
            TrainingImages = trainingImages;
            TestImages = testImages;
        }

        public ExecutionGraphModel? TrainFeedForwardNeuralNetwork(
            uint hiddenLayerSize = 1024,
            uint numIterations = 20,
            float trainingRate = 0.1f,
            uint batchSize = 128
        )
        {
            Console.Write("Loading MNIST...");
            using var trainer = GetVectorTrainer();
            Console.WriteLine("done");
            return trainer.TrainingFeedForwardNeuralNetwork(hiddenLayerSize, numIterations, trainingRate, batchSize);
        }

        public ExecutionGraphModel? TrainConvolutionalNeuralNetwork(
            uint hiddenLayerSize = 1024,
            uint numIterations = 20,
            float trainingRate = 0.1f,
            uint batchSize = 128
        )
        {
            Console.Write("Loading MNIST...");
            using var trainer = GetTensorTrainer();
            Console.WriteLine("done");
            return trainer.TrainConvolutionalNeuralNetwork(hiddenLayerSize, numIterations, trainingRate, batchSize);
        }

        public MnistVectorTrainer GetVectorTrainer()
        {
            return new(
                BuildVectorToVectorDataTable(_context, TrainingImages),
                BuildVectorToVectorDataTable(_context, TestImages)
            );
        }

        public MnistTensorTrainer GetTensorTrainer()
        {
            return new(
                Build3DTensorToVectorDataTable(_context, TrainingImages),
                Build3DTensorToVectorDataTable(_context, TestImages)
            );
        }

        /// <summary>
        /// Input layer size
        /// </summary>
        public const int InputSize = 784;

        /// <summary>
        /// Output layer size
        /// </summary>
        public const int OutputSize = 10;

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
            public (Vector<float> Data, Vector<float> Label) AsFloatArray(IBrightDataContext context)
            {
                var label = new float[10];
                label[_label] = 1;

                return (
                    context.CreateVector(_data.Select(b => Convert.ToSingle((int)b) / 255f).ToArray()),
                    context.CreateVector(label)
                );
            }

            /// <summary>
            /// Converts the image to a tensor with one hot encoded label vector
            /// </summary>
            public (Tensor3D<float> Tensor, Vector<float> Label) AsFloatTensor(IBrightDataContext context)
            {
                const int SIZE = 28;
                var data = AsFloatArray(context);
                var rows = new List<Vector<float>>();
                var vector = data.Data;

                for (var y = 0; y < SIZE; y++) {
                    var row = new float[SIZE];
                    for (var x = 0; x < SIZE; x++)
                        row[x] = vector[(y * SIZE) + x];
                    rows.Add(context.CreateVector(row));
                }

                var tensor = context.CreateTensor3D(context.CreateMatrixFromRows(rows.ToArray()));
                return (tensor, data.Label);
            }
        }

        public static Image[] Load(Stream labelStream, Stream imageStream, uint total = int.MaxValue)
        {
            var labels = new List<byte>();
            using (var reader = new BigEndianBinaryReader(labelStream)) {
                reader.ReadInt32();
                var count = reader.ReadUInt32();
                for (uint i = 0; i < count && i < total; i++) {
                    labels.Add(reader.ReadByte());
                }
            }

            var images = new List<byte[]>();
            using (var reader = new BigEndianBinaryReader(imageStream)) {
                reader.ReadInt32();
                var count = reader.ReadUInt32();
                var numRows = reader.ReadUInt32();
                var numCols = reader.ReadUInt32();
                var imageSize = numRows * numCols;
                for (uint i = 0; i < count && i < total; i++) {
                    var imageData = new byte[imageSize];
                    for (var j = 0; j < imageSize; j++) {
                        imageData[j] = reader.ReadByte();
                    }

                    images.Add(imageData);
                }
            }

            return labels.Zip(images, (l, d) => new Image(d, l)).ToArray();
        }

        public static IRowOrientedDataTable BuildVectorToVectorDataTable(IBrightDataContext context, Image[] images)
        {
            // create a vector => vector mapping
            var dataTable = context.CreateTwoColumnVectorTableBuilder();

            foreach (var image in images) {
                var (data, label) = image.AsFloatArray(context);
                dataTable.AddRow(data, label);
            }

            return dataTable.BuildRowOriented();
        }

        public static IRowOrientedDataTable Build3DTensorToVectorDataTable(IBrightDataContext context, Image[] images)
        {
            // create a 3D tensor => vector mapping
            var dataTable = context.Create3DTensorToVectorTableBuilder();

            foreach (var image in images) {
                var (tensor, label) = image.AsFloatTensor(context);
                dataTable.AddRow(tensor, label);
            }

            return dataTable.BuildRowOriented();
        }
    }
}
