using BrightWire;
using BrightWire.Descriptor.GradientDescent;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Component;
using BrightWire.ExecutionGraph.ErrorMetric;
using BrightWire.ExecutionGraph.Input;
using BrightWire.ExecutionGraph.Layer;
using BrightWire.ExecutionGraph.Wire;
using BrightWire.TrainingData.Artificial;
using System;
using System.Linq;

namespace ExampleCode
{
    class Program
    {
        static void Main(string[] args)
        {
            //var matrix = lap.Create(3, 3, (i, j) => (i+1) * (j+1));
            //var rot180 = matrix.Rotate180();

            //var xml = matrix.AsIndexable().AsXml;
            //var xml2 = rot180.AsIndexable().AsXml;

            using (var lap = Provider.CreateLinearAlgebra()) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.Rmse;
                var data = Xor.Get();

                // create the property set
                var propertySet = graph.CreatePropertySet();
                propertySet.Use(new RmsPropDescriptor(0.9f));

                var feeder = graph.CreateInput(data);
                var engine = graph.CreateTrainingEngine(0.03f, 2, feeder);

                const int HIDDEN_LAYER_SIZE = 4;
                var network = graph.Connect(feeder, propertySet)
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(graph.Activation.Sigmoid)
                    .AddFeedForward(null)
                    .Add(graph.Activation.Sigmoid)
                    .Add(new Backpropagate(errorMetric))
                    .Build()
                ;

                for (var i = 0; i < 1000; i++) {
                    var trainingError = engine.Train();
                    if(i % 100 == 0)
                        engine.Test(errorMetric);
                }
                engine.Test(errorMetric);

                var testData = data.SelectColumns(Enumerable.Range(0, 2));
                var testInput = graph.CreateInput(testData, testData.GetVectoriser(false));
                testInput.AddTarget(network);
                var executionEngine = graph.CreateEngine(testInput);
                var results = executionEngine.Execute();
            }
        }
    }
}