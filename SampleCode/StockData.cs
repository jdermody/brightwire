using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.ExecutionGraph;
using BrightWire.Models;
using ProtoBuf;

namespace BrightWire.SampleCode
{
	public partial class Program
	{
		/// <summary>
		/// Uses a recurrent LSTM neural network to predict stock price movements
		/// Data can be downloaded from https://raw.githubusercontent.com/plotly/datasets/master/stockdata.csv
		/// </summary>
		static void StockData(string dataFilePath)
		{
			// load and normalise the data
			var dataSet = new StreamReader(dataFilePath).ParseCSV(',', true);
			var normalised = dataSet.Normalise(NormalisationType.FeatureScale);
			var rows = normalised.GetNumericRows(dataSet.Columns.Where(c => c.Name != "Date").Select(c => c.Index));

			// build the data table with a window of input data and the prediction as the following value
			var builder = BrightWireProvider.CreateDataTableBuilder();
			builder.AddColumn(ColumnType.Matrix, "Past");
			builder.AddColumn(ColumnType.Vector, "Future");
			const int LAST_X_DAYS = 14;
			for (var i = 0; i < rows.Count - LAST_X_DAYS - 1; i++) {
				var inputVector = new List<FloatVector>();
				for (var j = 0; j < LAST_X_DAYS; j++)
					inputVector.Add(FloatVector.Create(rows[i + j]));
				var input = FloatMatrix.Create(inputVector.ToArray());
				var target = FloatVector.Create(rows[i+1]);
				builder.Add(input, target);
			}
			var data = builder.Build().Split(trainingPercentage: 0.2);

			using (var lap = BrightWireGpuProvider.CreateLinearAlgebra()) {
				var graph = new GraphFactory(lap);
				var errorMetric = graph.ErrorMetric.Quadratic;

				// create the property set
				graph.CurrentPropertySet
					.Use(graph.GradientDescent.Adam)
					.Use(graph.WeightInitialisation.Xavier);

				// create the engine
				var trainingData = graph.CreateDataSource(data.Training);
				var testData = trainingData.CloneWith(data.Test);
				var engine = graph.CreateTrainingEngine(trainingData, learningRate: 0.03f, batchSize: 128);

				// build the network
				const int HIDDEN_LAYER_SIZE = 256;
				graph.Connect(engine)
					.AddLstm(new float[HIDDEN_LAYER_SIZE])
					.AddFeedForward(engine.DataSource.OutputSize)
					.Add(graph.TanhActivation())
					.AddBackpropagationThroughTime(errorMetric);

				// train the network and restore the best result
				GraphModel bestNetwork = null;
				engine.Train(50, testData, errorMetric, model => bestNetwork = model);
				if (bestNetwork != null) {
					// execute each row of the test data on an execution engine
					var executionEngine = graph.CreateEngine(bestNetwork.Graph);
					var results = executionEngine.Execute(testData).OrderSequentialOutput();
					var expectedOutput = data.Test.GetColumn<FloatVector>(1);

					var score = results.Select((r, i) => errorMetric.Compute(r.Last(), expectedOutput[i])).Average();
					Console.WriteLine(score);
				}
			}
		}
	}
}
