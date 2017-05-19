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
            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.OneHotEncoding;
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                    .Use(graph.GaussianWeightInitialisation())
                ;
                var learningContext = graph.CreateLearningContext(0.0003f, 1, false, true);
                var executionContext = graph.CreateExecutionContext();

                Console.Write("Loading training data...");
                var trainingData = _BuildTensors(null, learningContext, executionContext, graph, Mnist.Load(dataFilesPath + "train-labels.idx1-ubyte", dataFilesPath + "train-images.idx3-ubyte", 800));
                var testData = _BuildTensors(trainingData, learningContext, executionContext, graph, Mnist.Load(dataFilesPath + "t10k-labels.idx1-ubyte", dataFilesPath + "t10k-images.idx3-ubyte", 200));
                Console.WriteLine($"done - {trainingData.RowCount} training images and {testData.RowCount} test images loaded");

                // create the network
                const int HIDDEN_LAYER_SIZE = 128;
                var engine = graph.CreateTrainingEngine(trainingData, executionContext, 0.0003f, 128);
                graph.Connect(engine)
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.ReluActivation())
                    .AddFeedForward(trainingData.OutputSize)
                    .Add(graph.SigmoidActivation())
                    .AddForwardAction(new Backpropagate(errorMetric))
                ;

                engine.Train(25, testData, errorMetric);
            }
        }

        static IDataSource _BuildTensors(IDataSource existing, ILearningContext learningContext, IExecutionContext executionContext, GraphFactory graph, IReadOnlyList<Mnist.Image> images)
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
            else {
                return graph.GetDataSource(dataTable.Build(), learningContext, executionContext, builder => builder
                    .AddConvolutional(1, 8, 1, 3, 3, 1)
                    .AddMaxPooling(2, 2, 2)
                    //.AddConvolutional(8, 4, 0, 3, 3, 2)
                    .Add(graph.ReluActivation())
                );
            }
        }

        
    }
}
