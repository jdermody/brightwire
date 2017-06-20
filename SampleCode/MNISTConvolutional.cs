using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
using BrightWire.Models;
using BrightWire.TrainingData;
using BrightWire.TrainingData.Artificial;
using BrightWire.TrainingData.WellKnown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static void MNISTConvolutional(string dataFilesPath)
        {
            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra()) {
                var graph = new GraphFactory(lap);

                Console.Write("Loading training data...");
                var trainingData = _BuildTensors(graph, null, Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte"));
                var testData = _BuildTensors(graph, trainingData, Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte"));
                Console.WriteLine($"done - {trainingData.RowCount} training images and {testData.RowCount} test images loaded");

                var errorMetric = graph.ErrorMetric.OneHotEncoding;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.RmsProp)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the network
                const int HIDDEN_LAYER_SIZE = 1024;
                const float LEARNING_RATE = 0.05f;
                var engine = graph.CreateTrainingEngine(trainingData, LEARNING_RATE, 32);
                engine.LearningContext.ScheduleLearningRate(15, LEARNING_RATE / 3);
                graph.Connect(engine)
                    .AddConvolutional(8, 2, 5, 5, 1, false)
                    .Add(graph.ReluActivation())
                    //.AddDropOut(0.5f)
                    .AddMaxPooling(2, 2, 2)
                    .AddConvolutional(16, 2, 5, 5, 1)
                    .Add(graph.ReluActivation())
                    .AddMaxPooling(2, 2, 2)
                    .Transpose()
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.ReluActivation())
                    //.AddDropOut(0.5f)
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SoftMaxActivation())
                    .AddBackpropagation(errorMetric)
                ;

                engine.Train(20, testData, errorMetric);

                // export the graph and verify that the error is the same
                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph);
                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }

        static IDataSource _BuildTensors(GraphFactory graph, IDataSource existing, IReadOnlyList<Mnist.Image> images)
        {
            var dataTable = BrightWireProvider.CreateDataTableBuilder();
            dataTable.AddColumn(ColumnType.Tensor, "Image");
            dataTable.AddColumn(ColumnType.Vector, "Target", true);

            foreach (var image in images) {
                var data = image.AsFloatTensor;
                dataTable.Add(data.Tensor, data.Label);
            }
            if (existing != null)
                return existing.CloneWith(dataTable.Build());
            else
                return graph.CreateDataSource(dataTable.Build());
        }
    }
}
