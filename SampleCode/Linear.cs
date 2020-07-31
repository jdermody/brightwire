using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.ExecutionGraph;

namespace BrightWire.SampleCode
{
	public partial class Program
	{
		public static void SimpleLinearTest()
		{
			Console.WriteLine($"\nRunning {Console.Title = nameof(SimpleLinearTest)}\n");

			var dataTableBuilder = BrightWireProvider.CreateDataTableBuilder();
			dataTableBuilder.AddColumn(ColumnType.Float, "capital costs");
			dataTableBuilder.AddColumn(ColumnType.Float, "labour costs");
			dataTableBuilder.AddColumn(ColumnType.Float, "energy costs");
			dataTableBuilder.AddColumn(ColumnType.Float, "output", true);

			dataTableBuilder.Add(98.288f, 0.386f, 13.219f, 1.270f);
			dataTableBuilder.Add(255.068f, 1.179f, 49.145f, 4.597f);
			dataTableBuilder.Add(208.904f, 0.532f, 18.005f, 1.985f);
			dataTableBuilder.Add(528.864f, 1.836f, 75.639f, 9.897f);
			dataTableBuilder.Add(307.419f, 1.136f, 52.234f, 5.907f);
			dataTableBuilder.Add(138.283f, 1.085f, 9.027f, 1.832f);
			dataTableBuilder.Add(418.883f, 2.390f, 1.676f, 4.865f);
			dataTableBuilder.Add(247.439f, 1.356f, 31.244f, 2.728f);
			dataTableBuilder.Add(19.478f, 0.115f, 1.739f, 0.125f);
			dataTableBuilder.Add(537.540f, 2.591f, 104.584f, 9.685f);
			dataTableBuilder.Add(605.507f, 2.789f, 82.296f, 8.727f);
			dataTableBuilder.Add(174.765f, 0.933f, 21.990f, 2.239f);
			dataTableBuilder.Add(946.766f, 4.004f, 125.351f, 10.077f);
			dataTableBuilder.Add(296.490f, 1.513f, 43.232f, 4.477f);
			dataTableBuilder.Add(645.690f, 2.540f, 75.581f, 7.037f);
			dataTableBuilder.Add(288.975f, 1.416f, 42.037f, 3.507f);

			var dataTable = dataTableBuilder.Build().Normalise(NormalisationType.Standard);

			using (var lap = BrightWireProvider.CreateLinearAlgebra(false))
			{
				var trainer = dataTable.CreateLinearRegressionTrainer(lap);
				var theta = trainer.GradientDescent(20, 0.03f, 0.1f, cost =>
				{
					Console.WriteLine(cost);
					return true;
				});
				Console.WriteLine(theta.Theta);
			}
		}

		static IDataTable _LoadBicyclesDataTable(string dataFilePath)
		{
			IDataTable ret;
			using (var reader = new StreamReader(dataFilePath))
			{
				var completeTable = reader.ParseCSV(',', true);

				// drop the first six columns (index and date features)
				var filteredTable = completeTable.SelectColumns(Enumerable.Range(0, completeTable.ColumnCount).Skip(5));

				// normalise just the data columns
				ret = filteredTable
					.SelectColumns(Enumerable.Range(0, filteredTable.ColumnCount - 3))
					.Normalise(NormalisationType.Standard)
				;

				// append the final classification label
				ret = ret.Zip(filteredTable.SelectColumns(new[] { filteredTable.ColumnCount - 1 }));
			}

			return ret;
		}

		/// <summary>
		/// Trains a linear regression model to predict bicycle sharing patterns
		/// Files can be downloaded from https://archive.ics.uci.edu/ml/machine-learning-databases/00275/
		/// </summary>
		/// <param name="dataFilePath">The path to the csv file</param>
		public static void PredictBicyclesWithLinearModel(string dataFilePath)
		{
			Console.WriteLine($"\nRunning {Console.Title = nameof(PredictBicyclesWithLinearModel)}\n");

			if (!File.Exists(dataFilePath))
			{
				var destFile = new FileInfo(dataFilePath);
				destFile.Directory.Parent.Create();
				var url = "https://archive.ics.uci.edu/ml/machine-learning-databases/00275/Bike-Sharing-Dataset.zip";
				Console.WriteLine($"Downloading data: {url}");
				var compressed = new FileInfo(Path.Combine(destFile.Directory.Parent.FullName, destFile.Directory.Name + ".zip"));
				new WebClient().DownloadFile(url, compressed.FullName);
				System.IO.Compression.ZipFile.ExtractToDirectory(compressed.FullName, destFile.Directory.FullName);
				Console.WriteLine($"Unzipped data: {destFile.Directory.FullName}");
				Console.WriteLine();
			}

			var dataTable = _LoadBicyclesDataTable(dataFilePath);
			var split = dataTable.Split(0);

			using (var lap = BrightWireProvider.CreateLinearAlgebra(false))
			{
				var trainer = split.Training.CreateLinearRegressionTrainer(lap);
				int iteration = 0;
				var theta = trainer.GradientDescent(500, 0.000025f, 0f, cost =>
				{
					if(iteration++ % 20 == 0)
						Console.WriteLine(cost);
					return true;
				});
				Console.WriteLine(theta.Theta);

				var testData = split.Test.GetNumericRows(Enumerable.Range(0, dataTable.ColumnCount - 1));
				var predictor = theta.CreatePredictor(lap);
				int index = 0;
				foreach (var row in testData)
				{
					var prediction = predictor.Predict(row);
					var actual = split.Test.GetRow(index++).GetField<float>(split.Test.TargetColumnIndex);
				}
			}
		}

		/// <summary>
		/// Trains a linear regression model to predict bicycle sharing patterns
		/// Files can be downloaded from https://archive.ics.uci.edu/ml/machine-learning-databases/00275/
		/// </summary>
		/// <param name="dataFilePath">The path to the csv file</param>
		public static void PredictBicyclesWithNeuralNetwork(string dataFilePath)
		{
			Console.WriteLine($"\nRunning {Console.Title = nameof(PredictBicyclesWithNeuralNetwork)}\n");

			if (!File.Exists(dataFilePath))
			{
				var destFile = new FileInfo(dataFilePath);
				destFile.Directory.Parent.Create();
				var url = "https://archive.ics.uci.edu/ml/machine-learning-databases/00275/Bike-Sharing-Dataset.zip";
				Console.WriteLine($"Downloading data: {url}");
				var compressed = new FileInfo(Path.Combine(destFile.Directory.Parent.FullName, destFile.Directory.Name + ".zip"));
				new WebClient().DownloadFile(url, compressed.FullName);
				System.IO.Compression.ZipFile.ExtractToDirectory(compressed.FullName, destFile.Directory.FullName);
				Console.WriteLine($"Unzipped data: {destFile.Directory.FullName}"); 
				Console.WriteLine();
			}

			var dataTable = _LoadBicyclesDataTable(dataFilePath);
			var split = dataTable.Split(0);

			using (var lap = BrightWireProvider.CreateLinearAlgebra(false))
			{
				var graph = new GraphFactory(lap);
				var errorMetric = graph.ErrorMetric.Quadratic;
				var trainingData = graph.CreateDataSource(split.Training);
				var testData = trainingData.CloneWith(split.Test);
				graph.CurrentPropertySet
					.Use(graph.RmsProp())
				;

				var engine = graph.CreateTrainingEngine(trainingData, 0.1f, 32);
				graph.Connect(engine)
					.AddFeedForward(16)
					.Add(graph.SigmoidActivation())
					//.AddDropOut(dropOutPercentage: 0.5f)
					.AddFeedForward(engine.DataSource.OutputSize)
					//.Add(graph.SigmoidActivation())
					.AddBackpropagation(errorMetric)
				;

				engine.Train(500, testData, errorMetric);
			}
		}
	}
}
