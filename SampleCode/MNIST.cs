using BrightWire.ExecutionGraph;
using BrightWire.TrainingData.WellKnown;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        /// <summary>
        /// Trains a feed forward neural net on the MNIST data set (handwritten digit recognition)
        /// The data files can be downloaded from http://yann.lecun.com/exdb/mnist/
        /// </summary>
        /// <param name="dataFilesPath">The path to a directory with the four extracted data files</param>
        public static void MNIST(string dataFilesPath)
        {
            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra()) {
                var graph = new GraphFactory(lap);

                Console.Write("Loading training data...");
                var trainingData = _BuildVectors(null, graph, Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte"));
                var testData = _BuildVectors(trainingData, graph, Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte"));
                Console.WriteLine($"done - {trainingData.RowCount} training images and {testData.RowCount} test images loaded");

                // one hot encoding uses the index of the output vector's maximum value as the classification label
                var errorMetric = graph.ErrorMetric.OneHotEncoding;

                // configure the network properties
                graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the training engine and schedule a training rate change
                const float TRAINING_RATE = 0.1f;
                var engine = graph.CreateTrainingEngine(trainingData, TRAINING_RATE, 128);
                engine.LearningContext.ScheduleLearningRate(15, TRAINING_RATE / 3);

                // create the network
                graph.Connect(engine)
                    .AddFeedForward(outputSize: 1024)
                    .Add(graph.LeakyReluActivation())
                    .AddDropOut(dropOutPercentage: 0.5f)
                    .AddFeedForward(outputSize: trainingData.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network for twenty iterations, saving the model on each improvement
                Models.ExecutionGraph bestGraph = null;
                engine.Train(20, testData, errorMetric, model => bestGraph = model.Graph);

                // export the final model and execute it on the training set
                var executionEngine = graph.CreateEngine(bestGraph ?? engine.Graph);
                var output = executionEngine.Execute(testData);
                Console.WriteLine($"Final accuracy: {output.Average(o => o.CalculateError(errorMetric)):P2}");
            }
        }

        static IDataSource _BuildVectors(IDataSource existing, GraphFactory graph, IReadOnlyList<Mnist.Image> images)
        {
            // feed forward neural networks expect a vector => vector mapping
            var dataTable = BrightWireProvider.CreateDataTableBuilder();
            dataTable.AddColumn(ColumnType.Vector, "Image");
            dataTable.AddColumn(ColumnType.Vector, "Target", isTarget: true);

            foreach (var image in images) {
                var data = image.AsFloatArray;
                dataTable.Add(data.Data, data.Label);
            }

            // reuse the network used for training when building the test data source
            if (existing != null)
                return existing.CloneWith(dataTable.Build());
            else
                return graph.CreateDataSource(dataTable.Build());
        }
    }
}
