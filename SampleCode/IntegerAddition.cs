using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Action;
using BrightWire.Models;
using BrightWire.TrainingData.Artificial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static void _WriteBinary(float value)
        {
            if (value >= 0.5)
                Console.Write("1");
            else
                Console.Write("0");
        }

        public static void IntegerAddition()
        {
            // generate 1000 random integer additions (split into training and test sets)
            var data = BinaryIntegers.Addition(1000, false).Split(0);
            using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);
                var errorMetric = graph.ErrorMetric.BinaryClassification;

                // modify the property set
                var propertySet = graph.CurrentPropertySet
                    .Use(graph.GradientDescent.Adam)
                    .Use(graph.WeightInitialisation.Xavier)
                ;

                // create the engine
                var trainingData = graph.GetDataSource(data.Training);
                var testData = trainingData.CloneWith(data.Test);
                var executionContext = graph.CreateExecutionContext();
                var engine = graph.CreateTrainingEngine(trainingData, executionContext, 0.0002f, 16);

                // build the network
                const int HIDDEN_LAYER_SIZE = 32;
                var memory = new float[HIDDEN_LAYER_SIZE];
                var network = graph.Connect(engine)
                    .AddSimpleRecurrent(graph.ReluActivation(), memory)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.ReluActivation())
                    .AddForwardAction(new BackpropagateThroughTime(errorMetric))
                ;

                // train the network
                engine.Train(30, testData, errorMetric);

                // export the graph and verify that the error is the same
                var networkGraph = engine.Graph;
                var executionEngine = graph.CreateEngine(networkGraph, executionContext);
                var output = executionEngine.Execute(testData);
                Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));
            }
        }
    }
}
