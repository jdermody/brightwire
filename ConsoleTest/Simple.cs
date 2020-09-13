using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Numerics;
using BrightWire;
using BrightWire.ExecutionGraph;

namespace ExampleCode
{
    static class Simple
    {
		public static void Xor(IBrightDataContext context)
        {
            var lap = context.UseNumericsLinearAlgebra();

			// Some training data that the network will learn.  The XOR pattern looks like:
			// 0 0 => 0
			// 1 0 => 1
			// 0 1 => 1
			// 1 1 => 0
			var data = BrightWire.TrainingData.Artificial.Xor.Get(context);

			// create the graph
			var graph = new GraphFactory(lap);
			var errorMetric = graph.ErrorMetric.CrossEntropy;
			graph.CurrentPropertySet
				// use rmsprop gradient descent optimisation
				.Use(graph.GradientDescent.RmsProp)

				// and gaussian weight initialisation
				.Use(graph.WeightInitialisation.Gaussian)
			;

			// create the engine
			var testData = graph.CreateDataSource(data);
			var engine = graph.CreateTrainingEngine(testData, learningRate: 0.1f, batchSize: 4);

			// create the network
			const int HIDDEN_LAYER_SIZE = 6;
			graph.Connect(engine)
				// create a feed forward layer with sigmoid activation
				.AddFeedForward(HIDDEN_LAYER_SIZE)
				.Add(graph.SigmoidActivation())

				// create a second feed forward layer with sigmoid activation
				.AddFeedForward(engine.DataSource.OutputSize ?? throw new Exception("No output"))
				.Add(graph.SigmoidActivation())

				// calculate the error and backpropagate the error signal
				.AddBackpropagation(errorMetric)
			;

			// train the network
			var executionContext = graph.CreateExecutionContext();
			for (var i = 0; i < 1000; i++) {
				engine.Train(executionContext);
				if (i % 100 == 0)
					engine.Test(testData, errorMetric);
			}
			engine.Test(testData, errorMetric);

			// create a new network to execute the learned network
			var networkGraph = engine.Graph;
			var executionEngine = graph.CreateEngine(networkGraph);
			var output = executionEngine.Execute(testData);
			Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));

			// print the values that have been learned
			foreach (var item in output) {
				foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
					var row = data.Row(index);
					var result = item.Output[index];
					Console.WriteLine($"{row[0]} XOR {row[1]} = {result[0]}");
				}
			}
		}
	}
}
