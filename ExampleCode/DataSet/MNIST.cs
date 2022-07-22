using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Cuda;
using BrightData.Cuda.Helper;
using BrightData.LinearAlgebra;
using BrightWire.Models;
using BrightWire.TrainingData.Helper;
using ExampleCode.DataTableTrainers;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace ExampleCode.DataSet
{
    internal class Mnist : IDisposable
    {
        readonly BrightDataContext _context;
        public Image[] TrainingImages { get; }
        public Image[] TestImages { get; }
        readonly List<IDisposable> _tensorCache = new();

        public Mnist(BrightDataContext context, Image[] trainingImages, Image[] testImages)
        {
            _context = context;
            TrainingImages = trainingImages;
            TestImages = testImages;
        }

        public void Dispose()
        {
            _tensorCache.DisposeAll();
        }

        public ExecutionGraphModel? TrainFeedForwardNeuralNetwork(
            uint hiddenLayerSize = 1024,
            uint numIterations = 1,
            float trainingRate = 0.003f,
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
            float trainingRate = 0.001f,
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
            internal Image(byte[] data, int label)
            {
                Data = data;
                Label = label;
            }

            /// <summary>
            /// The image data
            /// </summary>
            public byte[] Data { get; }

            /// <summary>
            /// The image number (0-9)
            /// </summary>
            public int Label { get; }

            /// <summary>
            /// Converts the image to one hot encoded float arrays
            /// </summary>
            public (IReadOnlyVector Data, IReadOnlyVector Label) AsFloatArray(BrightDataContext context)
            {
                return (
                    context.CreateVectorInfo(Data.Length, i => Data[i] / 255f),
                    context.CreateVectorInfo(10, i => i == Label ? 1f : 0f)
                );
            }

            /// <summary>
            /// Converts the image to a tensor with one hot encoded label vector
            /// </summary>
            public (IReadOnlyTensor3D Tensor, IReadOnlyVector Label) AsFloatTensor(BrightDataContext context)
            {
                const int SIZE = 28;
                var (vector, label) = AsFloatArray(context);
                var rows = new List<IReadOnlyVector>();

                for (var y = 0; y < SIZE; y++) {
                    var row = new float[SIZE];
                    for (var x = 0; x < SIZE; x++)
                        row[x] = vector[(y * SIZE) + x];
                    rows.Add(context.CreateVectorInfo(row));
                }

                var tensor = context.CreateTensor3D(context.CreateMatrixInfoFromRows(rows.ToArray()));
                return (tensor, Label: label);
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

        public BrightDataTable BuildVectorToVectorDataTable(BrightDataContext context, Image[] images)
        {
            // create a vector => vector mapping
            var builder = context.CreateTwoColumnVectorTableBuilder();

            foreach (var image in images) {
                var (data, label) = image.AsFloatArray(context);
                builder.AddRow(data, label);
            }

            var ret = builder.BuildInMemory();
            if (context.LinearAlgebraProvider.IsCuda(out var cudaProvider))
                _tensorCache.Add(new CudaTensorDataCache(cudaProvider, ret));
            return ret;
        }

        public static BrightDataTable Build3DTensorToVectorDataTable(BrightDataContext context, Image[] images)
        {
            // create a 3D tensor => vector mapping
            var dataTable = context.Create3DTensorToVectorTableBuilder();

            foreach (var image in images) {
                var (tensor, label) = image.AsFloatTensor(context);
                dataTable.AddRow(tensor, label);
            }

            return dataTable.BuildInMemory();
        }
    }
}
