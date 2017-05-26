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
            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra()) {
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

                // create the training engine and schedule two training rate changes
                const float TRAINING_RATE = 0.003f;
                var engine = graph.CreateTrainingEngine(trainingData, TRAINING_RATE, 128);
                engine.LearningContext.ScheduleLearningRate(15, TRAINING_RATE / 3);
                engine.LearningContext.ScheduleLearningRate(20, TRAINING_RATE / 9);

                // create the network
                graph.Connect(engine)
                    .AddFeedForward(1024)
                    .Add(graph.LeakyReluActivation())
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network for 30 epochs
                engine.Train(25, testData, errorMetric);

                // export the graph and verify that the error is the same
                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);
                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
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
