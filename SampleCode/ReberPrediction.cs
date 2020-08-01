using BrightWire.ExecutionGraph;
using BrightWire.TrainingData.Artificial;
using System;
using System.Linq;
using BrightWire.LinearAlgebra.Helper;
using BrightWire.Models;
using MathNet.Numerics.Distributions;

namespace BrightWire.SampleCode
{
	public partial class Program
	{
		static void ReberPrediction()
		{
			Console.WriteLine($"\nRunning {Console.Title = nameof(ReberPrediction)}\n");

			// generate 500 extended reber grammar training examples
			var grammar = new ReberGrammar();
			var sequences = grammar.GetExtended(10, 16).Take(500).ToList();

			// split the data into training and test sets
			var data = ReberGrammar.GetOneHot(sequences).Split(0);

			using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
				var graph = new GraphFactory(lap);

				// binary classification rounds each output to either 0 or 1
				var errorMetric = graph.ErrorMetric.BinaryClassification;

				// configure the network properties
				graph.CurrentPropertySet
					.Use(graph.GradientDescent.RmsProp)
					.Use(graph.WeightInitialisation.Xavier)
				;

				// create the engine
				var trainingData = graph.CreateDataSource(data.Training);
				var testData = trainingData.CloneWith(data.Test);
				var engine = graph.CreateTrainingEngine(trainingData, learningRate: 0.1f, batchSize: 32);

				// build the network
				const int HIDDEN_LAYER_SIZE = 32, TRAINING_ITERATIONS = 30;
				graph.Connect(engine)
					.AddGru(HIDDEN_LAYER_SIZE)
					.AddFeedForward(engine.DataSource.OutputSize)
					.Add(graph.SigmoidActivation())
					.AddBackpropagationThroughTime(errorMetric)
				;

				engine.Train(TRAINING_ITERATIONS, testData, errorMetric);

				// generate a sample sequence using the learned state transitions
				var networkGraph = engine.Graph;

				var executionEngine = graph.CreateEngine(networkGraph);

				Console.WriteLine("Generating new reber sequences from the observed state probabilities...");
				for (var z = 0; z < 3; z++) {
					// prepare the first input
					var input = new float[ReberGrammar.Size];
					input[ReberGrammar.GetIndex('B')] = 1f;
					Console.Write("B");

					int index = 0, eCount = 0;
					using (var executionContext = graph.CreateExecutionContext()) {
						var result = executionEngine.ExecuteSequential(index++, input, executionContext, MiniBatchSequenceType.SequenceStart);
						for (var i = 0; i < 32; i++) {
							var next = result.Output[0].Data
								.Select((v, j) => ((double)v, j))
								.Where(d => d.Item1 >= 0.1f)
								.ToList();

							var distribution = new Categorical(next.Select(d => d.Item1).ToArray());
							var nextIndex = next[distribution.Sample()].Item2;
							Console.Write(ReberGrammar.GetChar(nextIndex));
							if (nextIndex == ReberGrammar.GetIndex('E') && ++eCount == 2)
								break;

							Array.Clear(input, 0, ReberGrammar.Size);
							input[nextIndex] = 1f;
							result = executionEngine.ExecuteSequential(index++, input, executionContext, MiniBatchSequenceType.Standard);
						}

						Console.WriteLine();
					}
				}
			}
		}
	}
}
