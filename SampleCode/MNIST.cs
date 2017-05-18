using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
using BrightWire.Helper;
using BrightWire.TrainingData;
using BrightWire.TrainingData.WellKnown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        /// <summary>
        /// Trains a neural net on the MNIST database (digit recognition)
        /// The data files can be downloaded from http://yann.lecun.com/exdb/mnist/
        /// </summary>
        /// <param name="dataFilesPath">The path to a directory with the four extracted data files</param>
        public static void MNIST(string dataFilesPath)
        {
            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);

                // use a one hot encoding error metric, rmsprop gradient descent and xavier weight initialisation
                var errorMetric = graph.ErrorMetric.OneHotEncoding;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                Console.Write("Loading training data...");
                var trainingData = _BuildVectors(null, graph, Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte"));
                var testData = _BuildVectors(trainingData, graph, Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte"));
                Console.WriteLine($"done - {trainingData.RowCount} training images and {testData.RowCount} test images loaded");

                // create the training engine and schedule two learning rate changes
                var executionContext = graph.CreateExecutionContext();
                var engine = graph.CreateTrainingEngine(trainingData, executionContext, 0.001f, 128);
                engine.LearningContext.ScheduleLearningRate(10, 0.0003f);
                engine.LearningContext.ScheduleLearningRate(20, 0.0001f);

                // create the network
                graph.Connect(engine)
                    .AddFeedForward(1024)
                    .Add(graph.LeakyReluActivation())
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddForwardAction(new Backpropagate(errorMetric))
                ;

                // train the network for 30 epochs
                engine.Train(30, testData, errorMetric);
            }
        }

        static IDataSource _BuildVectors(IDataSource existing, GraphFactory graph, IReadOnlyList<Mnist.Image> images)
        {
            var dataTable = BrightWireProvider.CreateDataTableBuilder();
            dataTable.AddColumn(ColumnType.Vector, "Image");
            dataTable.AddColumn(ColumnType.Vector, "Target", true);

            foreach (var image in images) {
                var data = image.AsFloatArray;
                dataTable.Add(data.Data, data.Label);
            }
            if (existing != null)
                return existing.CloneWith(dataTable.Build());
            else
                return graph.GetDataSource(dataTable.Build());
        }
    }
}
