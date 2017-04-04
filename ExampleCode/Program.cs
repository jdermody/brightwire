using BrightWire;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Component;
using BrightWire.ExecutionGraph.Descriptor;
using BrightWire.ExecutionGraph.ErrorMetric;
using BrightWire.ExecutionGraph.Input;
using BrightWire.ExecutionGraph.Layer;
using BrightWire.ExecutionGraph.Wire;
using BrightWire.TrainingData.Artificial;
using System;

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
                var graph = new GraphFactory(lap, false);
                var propertySet = graph.GetPropertySet();
               
                const int HIDDEN_LAYER_SIZE = 4;

                var data = Xor.Get();
                var feeder = graph.CreateInput(data);
                
                var sigmoid = new Sigmoid();
                var errorMetric = new OneHotEncoding();

                var network = graph.GetConnector(feeder.InputSize, propertySet)
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
                    .Add(sigmoid)
                    .AddFeedForward(feeder.OutputSize)
                    .Add(sigmoid)
                    .Add(new Backpropagate(errorMetric))
                    .Build()
                ;
                feeder.AddTarget(network);

                //var layer1 = graph.CreateFeedForward(feeder.InputSize, HIDDEN_LAYER_SIZE, propertySet);
                //var layer2 = graph.CreateFeedForward(HIDDEN_LAYER_SIZE, feeder.OutputSize, propertySet);
                //var backProp = new Backpropagate(errorMetric);

                //var toBackProp = new ToComponent(backProp);
                //var tolayer2Activation = new LayerToWire(sigmoid, toBackProp);
                //var toLayer2 = new LayerToWire(layer2, tolayer2Activation);
                //var toLayer1Activation = new LayerToWire(sigmoid, toLayer2);
                //var toLayer1 = new LayerToWire(layer1, toLayer1Activation);
                //feeder.AddTarget(toLayer1);

                var context = new Context(0.03f, 2, true, false);
                for (var i = 0; i < 1000; i++) {
                    var trainingError = feeder.Execute(context, true);
                    Console.WriteLine($"Epoch {i}: Training error: {trainingError}");
                }
            }
        }
    }
}