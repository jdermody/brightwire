using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightWire.Models;
using BrightWire.TrainingData.Helper;
using ExampleCode.DataTableTrainers;

namespace ExampleCode.DataSet
{
    internal class Mnist(BrightDataContext context, Mnist.Image[] trainingImages, Mnist.Image[] testImages)
        : IDisposable
    {
        public Image[] TrainingImages { get; } = trainingImages;
        public Image[] TestImages { get; } = testImages;
        readonly List<IDisposable> _tensorCache = [];

        public void Dispose()
        {
            _tensorCache.DisposeAll();
        }

        public ExecutionGraphModel? TrainFeedForwardNeuralNetwork(
            uint hiddenLayerSize = 1024,
            uint numIterations = 10,
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
                BuildVectorToVectorDataTable(context, TrainingImages),
                BuildVectorToVectorDataTable(context, TestImages)
            );
        }

        public MnistTensorTrainer GetTensorTrainer()
        {
            return new(
                Build3DTensorToVectorDataTable(context, TrainingImages),
                Build3DTensorToVectorDataTable(context, TestImages)
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
                    context.CreateReadOnlyVector(Data.Length, i => Data[i] / 255f),
                    context.CreateReadOnlyVector(10, i => i == Label ? 1f : 0f)
                );
            }

            /// <summary>
            /// Converts the image to a tensor with one hot encoded label vector
            /// </summary>
            public (IReadOnlyTensor3D Tensor, IReadOnlyVector Label) AsFloatTensor(BrightDataContext context)
            {
                const int imageSize = 28;
                var (vector, label) = AsFloatArray(context);
                var rows = new IReadOnlyVector[imageSize];

                for (var y = 0; y < imageSize; y++) {
                    var row = new float[imageSize];
                    for (var x = 0; x < imageSize; x++)
                        row[x] = vector[(y * imageSize) + x];
                    rows[y] = context.CreateReadOnlyVector(row);
                }

                var imageAsMatrix = context.CreateReadOnlyMatrixFromRows(rows);
                var tensor = context.CreateReadOnlyTensor3D(imageAsMatrix);
                return (tensor, Label: label);
            }
        }

        public static Image[] Load(Stream labelStream, Stream imageStream, uint total = int.MaxValue)
        {
            byte[] labels;
            using (var reader = new BigEndianBinaryReader(labelStream)) {
                reader.ReadInt32();
                var count = reader.ReadUInt32();
                labels = reader.ReadBytes((int)Math.Min(count, total));
            }

            byte[][] images;
            using (var reader = new BigEndianBinaryReader(imageStream)) {
                reader.ReadInt32();
                var count = reader.ReadUInt32();
                var numRows = reader.ReadUInt32();
                var numCols = reader.ReadUInt32();
                var imageSize = numRows * numCols;
                images = new byte[(int)Math.Min(count, total)][];
                for (uint i = 0; i < count && i < total; i++)
                    images[i] = reader.ReadBytes((int)imageSize);
            }

            return labels.Zip(images, (l, d) => new Image(d, l)).ToArray();
        }

        public static IDataTable BuildVectorToVectorDataTable(BrightDataContext context, Image[] images)
        {
            // create a vector => vector mapping
            var builder = context.CreateTwoColumnVectorTableBuilder();

            foreach (var image in images) {
                var (data, label) = image.AsFloatArray(context);
                builder.AddRow(data, label);
            }

            var ret = builder.BuildInMemory();
            //if (context.LinearAlgebraProvider.IsCuda(out var cudaProvider))
            //    _tensorCache.Add(new CudaTensorDataCache(cudaProvider, ret));
            return ret.Result;
        }

        public IDataTable Build3DTensorToVectorDataTable(BrightDataContext context, Image[] images)
        {
            // create a 3D tensor => vector mapping
            var builder = context.Create3DTensorToVectorTableBuilder();

            foreach (var image in images) {
                var (tensor, label) = image.AsFloatTensor(context);
                builder.AddRow(tensor, label);
            }

            var ret = builder.BuildInMemory().Result;
            //if (context.LinearAlgebraProvider.IsCuda(out var cudaProvider))
            //    _tensorCache.Add(new CudaTensorDataCache(cudaProvider, ret));
            return ret;
        }
    }
}
