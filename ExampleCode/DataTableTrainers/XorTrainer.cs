using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData.Numerics;
using BrightTable;
using BrightWire;
using BrightWire.ExecutionGraph;
using BrightWire.Models;

namespace ExampleCode.DataTableTrainers
{
    class XorTrainer : DataTableTrainer
    {
        public XorTrainer(IRowOrientedDataTable table) : base(table, table, table)
        {
        }

        public ExecutionGraph TrainSigmoidNeuralNetwork(uint hiddenLayerSize, uint numIterations, float learningRate, uint batchSize, bool writeResults = true)
        {
            var context = Table.Context;

            // create the graph
            var graph = context.CreateGraphFactory();
            var errorMetric = graph.ErrorMetric.CrossEntropy;
            graph.CurrentPropertySet
                // use rmsprop gradient descent optimisation
                .Use(graph.GradientDescent.RmsProp)

                // and gaussian weight initialisation
                .Use(graph.WeightInitialisation.Gaussian)
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(Training);
            var engine = graph.CreateTrainingEngine(trainingData, learningRate, batchSize);

            // create the network
            graph.Connect(engine)
                // create a feed forward layer with sigmoid activation
                .AddFeedForward(hiddenLayerSize)
                .Add(graph.SigmoidActivation())

                .AddDropOut(dropOutPercentage: 0.5f)

                // create a second feed forward layer with sigmoid activation
                .AddFeedForward(engine.DataSource.OutputSize ?? throw new Exception("No output"))
                .Add(graph.SigmoidActivation())

                // calculate the error and backpropagate the error signal
                .AddBackpropagation(errorMetric)
            ;

            // train the network
            var executionContext = graph.CreateExecutionContext();
            var testData = graph.CreateDataSource(Test);
            engine.Test(testData, errorMetric);
            for (var i = 0; i < numIterations; i++) {
                engine.Train(executionContext);
                //engine.Test(testData, errorMetric);
            }

            // create a new network to execute the learned network
            var networkGraph = engine.Graph;
            var executionEngine = graph.CreateEngine(networkGraph);
            var output = executionEngine.Execute(testData).ToList();
            if (writeResults) {
                var testAccuracy = output.Average(o => o.CalculateError(graph.ErrorMetric.OneHotEncoding));
                Console.WriteLine($"Neural network accuracy: {testAccuracy:P}");

                // print the values that have been learned
                foreach (var item in output) {
                    foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
                        var row = Test.Row(index);
                        var result = item.Output[index];
                        Console.WriteLine($"{row} = {result}");
                    }
                }
            }

            return networkGraph;
        }
    }
}
