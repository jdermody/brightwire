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
            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.OneHotEncoding;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                    .Use(graph.XavierWeightInitialisation())
                ;

                Console.Write("Loading training data...");
                var trainingData = _BuildTensors(graph, null, Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte"));
                var testData = _BuildTensors(graph, trainingData, Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte"));
                Console.WriteLine($"done - {trainingData.RowCount} training images and {testData.RowCount} test images loaded");

                // create the network
                const int HIDDEN_LAYER_SIZE = 128;
                var engine = graph.CreateTrainingEngine(trainingData, 0.003f, 32);
                graph.Connect(engine)
                    .AddConvolutional(1, 8, 1, 3, 3, 1, false)
                    //.AddMaxPooling(2, 2, 2)
                    //.Add(graph.ReluActivation())
                    .AddConvolutional(8, 4, 1, 3, 3, 2)
                    .Add(graph.ReluActivation())
                    .Transpose(784)
                    .AddDropConnect(0.3f, HIDDEN_LAYER_SIZE)
                    .Add(graph.ReluActivation())
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SigmoidActivation())
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
                return graph.GetDataSource(dataTable.Build());
        }
    }
}
