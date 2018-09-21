using BrightWire.ExecutionGraph;
using BrightWire.TrainingData.Artificial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using BrightWire.Source.Helper;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace BrightWire.SampleCode
{
	partial class Program
	{
		public static void XOR()
		{
			using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
				// Some training data that the network will learn.  The XOR pattern looks like:
				// 0 0 => 0
				// 1 0 => 1
				// 0 1 => 1
				// 1 1 => 0
				var data = Xor.Get();

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
				var engine = graph.CreateTrainingEngine(testData, learningRate: 0.1f, batchSize:4);

				// create the network
				const int HIDDEN_LAYER_SIZE = 6;
				graph.Connect(engine)
					// create a feed forward layer with sigmoid activation
					.AddFeedForward(HIDDEN_LAYER_SIZE)
					.Add(graph.SigmoidActivation())

					// create a second feed forward layer with sigmoid activation
					.AddFeedForward(engine.DataSource.OutputSize)
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
						var row = data.GetRow(index);
						var result = item.Output[index];
						Console.WriteLine($"{row.GetField<int>(0)} XOR {row.GetField<int>(1)} = {result.Data[0]}");
					}
				}
			}
		}

		static void Main(string[] args)
		{
			// base path to a directory on your computer with training files
			const string DataBasePath = @"c:\data\";

			// base path to a directory on your computer to store model files
			const string ModelBasePath = @"c:\temp\";

			// uncomment to use the (faster) native MKL provider if available
			Control.UseNativeMKL();

			//XOR();
			//IrisClassification();
			//IrisClustering();
			//MarkovChains();
			//MNIST(DataBasePath + @"mnist\");
			//MNISTConvolutional(DataBasePath + @"mnist\"/*, ModelBasePath + @"mnist.dat"*/);
			//SentimentClassification(DataBasePath + @"sentiment labelled sentences\");
			//TextClustering(DataBasePath + @"[UCI] AAAI-14 Accepted Papers - Papers.csv", ModelBasePath);
			//IntegerAddition();
			//ReberPrediction();
			//OneToMany();
			//ManyToOne();
			//SequenceToSequence();
			//TrainWithSelu(DataBasePath + @"iris.data");
			//SimpleLinearTest();
			//PredictBicyclesWithLinearModel(DataBasePath + @"bikesharing\hour.csv");
			//PredictBicyclesWithNeuralNetwork(DataBasePath + @"bikesharing\hour.csv");
			//MultiLabelSingleClassifier(DataBasePath + @"emotions\emotions.arff");
			//MultiLabelMultiClassifiers(DataBasePath + @"emotions\emotions.arff");
			//return;

			//using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
			//	var rand = new Random();
			//	var list = new List<IVector>();
			//	Console.Write("Loading...");
			//	const int VECTOR_COUNT = 1024, VECTOR_SIZE = 4096;
			//	for (var i = 0; i < VECTOR_COUNT; i++) {
			//		var vector = lap.CreateVector(VECTOR_SIZE, j => (float)rand.NextDouble());
			//		list.Add(vector);
			//	}
			//	Console.WriteLine("done");

			//	var stopwatch = new Stopwatch();
			//	stopwatch.Start();

			//	var clusters = list.KMeans(lap, 50);
			//	Console.WriteLine(stopwatch.ElapsedMilliseconds);
			//}

			// load and normalise the data
			//var dataSet = new StreamReader(@"C:\Users\jack\Desktop\lstm\XBTEUR.csv").ParseCSV(';', true);
			//var columnNames = new[] { "Open", "High", "Low", "Volume", "Close" };
			//var columnsOfInterest = new HashSet<string>(columnNames);
			//var columnIndices = dataSet.Columns.Select((c, i) => new { Column = c, Index = i })
			//	.Where(c => columnsOfInterest.Contains(c.Column.Name))
			//	.OrderBy(c => Array.FindIndex(columnNames, n => c.Column.Name == n))
			//	.Select(c => c.Index)
			//	.ToList();
			//var analysis = dataSet.GetAnalysis();
			//var columnsMax = columnIndices.Select(ind => ((INumericColumnInfo)analysis[ind]).Max).ToList();
			//var rows = dataSet.Map(row => columnIndices.Select(row.GetField<float>).ToList());
			//var normalisedRows = rows.Select(row => row.Select((v, i) => Convert.ToSingle(v / columnsMax[i])).ToArray()).ToList();

			//// build the data table with a window of input data and the prediction as the following value
			//var builder = BrightWireProvider.CreateDataTableBuilder();
			//builder.AddColumn(ColumnType.Matrix, "Past");
			//builder.AddColumn(ColumnType.Vector, "Future");
			//const int PREDICTION_LENGTH = 30;
			//for (var i = PREDICTION_LENGTH + 1; i < rows.Count; i++) {
			//	var inputVector = new List<FloatVector>();
			//	for (var j = i - PREDICTION_LENGTH - 1; j < i - 1; j++)
			//		inputVector.Add(FloatVector.Create(normalisedRows[j]));
			//	var input = FloatMatrix.Create(inputVector.ToArray());
			//	var target = FloatVector.Create(normalisedRows[i]);
			//	builder.Add(input, target);
			//}

			//var data = builder.Build().Split(trainingPercentage: 0.2);

			//var TestData = data.Training;
			//var TrainingData = data.Test;

			//using (var lap = BrightWireProvider.CreateLinearAlgebra(false)) 
			//{
			//	var graph = new GraphFactory(lap);
			//	var errorMetric = graph.ErrorMetric.CrossEntropy;

			//	// create the property set
			//	var propertySet = graph.CurrentPropertySet
			//		.Use(graph.GradientDescent.Adam)
			//		.Use(graph.WeightInitialisation.Xavier);

			//	// create the engine
			//	var trainingData = graph.CreateDataSource(TrainingData);
			//	var testData = trainingData.CloneWith(TestData);
			//	var engine = graph.CreateTrainingEngine(trainingData, learningRate: 0.03f, batchSize: 128);

			//	// build the network
			//	const int HIDDEN_LAYER_SIZE = 128;
			//	var memory = new float[HIDDEN_LAYER_SIZE];
			//	var network = graph.Connect(engine)
			//		.AddLstm(memory)
			//		.AddFeedForward(engine.DataSource.OutputSize)
			//		.Add(graph.SigmoidActivation())
			//		.AddBackpropagationThroughTime(errorMetric);

			//	// train the network and restore the best result
			//	GraphModel bestNetwork = null;
			//	engine.Train(20, testData, errorMetric, model => bestNetwork = model);
			//	if (bestNetwork != null)
			//		engine.LoadParametersFrom(bestNetwork.Graph);

			//	// execute each row of the test data
			//	//var results = engine.Execute(testData).OrderSequentialOutput();
			//	//var score = results.Select((r, i) => errorMetric.Compute(r.Last(), TestData.GetRow(i).GetField<FloatVector>(1))).Average();
			//	//Console.WriteLine(score);

			//	TestData.ForEach(row => {
			//		var input = row.GetField<FloatMatrix>(0);
			//		var output = row.GetField<FloatVector>(1);
			//		var expectedOutput = output.Data.Last();
			//		var actualOutput = engine.ExecuteSequential(input.Row.Select(v => v.Data).ToList()).Last().Output[0].Data.Last();
			//		Console.WriteLine($"Expected value: {expectedOutput} Actual value: {actualOutput }");
			//	});
			//}

			//var distribution = new Normal(0, 1);
			//int inputDepth = 3, filterWidth = 2, filterHeight = 2, filterCount = 5, inputWidth = 4, inputHeight = 4, stride = 2, count = 6;
			//var _cpu = BrightWireProvider.CreateLinearAlgebra();
			//using (var _cuda = BrightWireGpuProvider.CreateLinearAlgebra()) {
			//	var cpuTensor = _cpu.Create4DTensor(Enumerable.Range(0, count).Select(z =>
			//		_cpu.Create3DTensor(Enumerable.Range(0, inputDepth).Select(i =>
			//			_cpu.CreateMatrix(inputWidth, inputHeight, (j, k) =>
			//				//Convert.ToSingle((i + 1) * (j + 1) * (k + 1) * (z + 1)))
			//				Convert.ToSingle(distribution.Sample()))
			//			).ToList()
			//		)
			//	).ToList());
			//	using (var gpuTensor = _cuda.Create4DTensor(cpuTensor.Data)) //{
			//																 //var cpuPadding = cpuTensor.AddPadding(2);
			//																 //var gpuPadding = gpuTensor.AddPadding(2);
			//																 //var padding = _WriteComparison(cpuPadding.AsIndexable(), gpuPadding.AsIndexable());

			//	//var cpuPadding2 = cpuPadding.RemovePadding(2);
			//	//var gpuPadding2 = gpuPadding.RemovePadding(2);
			//	//var padding2 = _WriteComparison(cpuPadding2.AsIndexable(), gpuPadding2.AsIndexable());

			//	//	var cpuMaxPool = cpuTensor.MaxPool(filterWidth, filterHeight, stride, true);
			//	//	var gpuMaxPool = gpuTensor.MaxPool(filterWidth, filterHeight, stride, true);
			//	//	var maxPool = _WriteComparison(cpuMaxPool.Result.AsIndexable(), gpuMaxPool.Result.AsIndexable());
			//	//	var indices = _WriteComparison(cpuMaxPool.Indices.AsIndexable(), gpuMaxPool.Indices.AsIndexable());

			//	//	var cpuReverseMaxPool = cpuMaxPool.Result.ReverseMaxPool(cpuMaxPool.Indices, cpuTensor.RowCount, cpuTensor.ColumnCount, filterWidth, filterHeight, stride);
			//	//	var gpuReverseMaxPool = gpuMaxPool.Result.ReverseMaxPool(gpuMaxPool.Indices, gpuTensor.RowCount, gpuTensor.ColumnCount, filterWidth, filterHeight, stride);
			//	//	var reverseMaxPool = _WriteComparison(cpuReverseMaxPool.AsIndexable(), gpuReverseMaxPool.AsIndexable());
			//	//}
			//	using (var cpuInput = cpuTensor.Im2Col(filterWidth, filterHeight, stride))
			//	using (var gpuInput = gpuTensor.Im2Col(filterWidth, filterHeight, stride)) {
			//		var im2Col = cpuInput.AsIndexable();
			//		var im2ColGpu = gpuInput.AsIndexable();
			//		var matrix = _WriteComparison(im2Col, im2ColGpu);

			//		var normalDistribution = new Normal(0, 1);
			//		var cpuFilter = _cpu.CreateMatrix(inputDepth * filterWidth * filterHeight, filterCount, (i, j) => (float)normalDistribution.Sample());
			//		var output = im2Col.Multiply(cpuFilter);

			//		var newWidth = ((inputWidth - filterWidth) / stride) + 1;
			//		var newHeight = ((inputHeight - filterHeight) / stride) + 1;
			//		var outputTensor = output.ReshapeAsVector().ReshapeAs4DTensor(newHeight, newWidth, filterCount, count);

			//		//var matrixList = new List<IMatrix>();
			//		//var newWidth = ((inputWidth - filterWidth) / stride) + 1;
			//		//var newHeight = ((inputHeight - filterHeight) / stride) + 1;
			//		//for (var i = 0; i < output.ColumnCount; i++)
			//		//	matrixList.Add(output.Column(i).AsMatrix(newWidth, newHeight));
			//		//var outputTensor = _cpu.Create3DTensor(matrixList);

			//		using(var gpuFilter = _cuda.CreateMatrix(cpuFilter.Data))
			//		using (var gpuOutputTensor = _cuda.Create4DTensor(outputTensor.Data)) {
			//			var reverseIm2Col = outputTensor.ReverseIm2Col(cpuFilter, inputHeight, inputWidth, inputDepth, filterWidth, filterHeight, stride);
			//			using (var gpuReverseIm2Col = gpuOutputTensor.ReverseIm2Col(gpuFilter, inputHeight, inputWidth, inputDepth, filterWidth, filterHeight, stride)) {
			//				var comparison = _WriteComparison(reverseIm2Col.AsIndexable(), gpuReverseIm2Col.AsIndexable());
			//			}
			//		}
			//	}
			//}
			//var matrix = _TensorReverseIm2Col(2, 2, 2, 2, 1, 4, 4);
		}

		static void _Write(IIndexableMatrix m1, IIndexableMatrix m2, XmlTextWriter writer)
		{
			writer.WriteStartElement("matrix");
			writer.WriteAttributeString("r1", m1.RowCount.ToString());
			writer.WriteAttributeString("r2", m2.RowCount.ToString());
			foreach (var row in m1.Rows.Zip(m2.Rows, (x, y) => (x, y))) {
				writer.WriteStartElement("row");
				writer.WriteAttributeString("c1", row.Item1.Count.ToString());
				writer.WriteAttributeString("c2", row.Item2.Count.ToString());
				foreach (var cell in row.Item1.Values.Zip(row.Item2.Values, (i, j) => (i, j))) {
					writer.WriteStartElement("cell");
					writer.WriteAttributeString("v1", cell.Item1.ToString(CultureInfo.InvariantCulture));
					writer.WriteAttributeString("v2", cell.Item2.ToString(CultureInfo.InvariantCulture));
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		static void _Write(IIndexable3DTensor t1, IIndexable3DTensor t2, XmlTextWriter writer)
		{
			writer.WriteStartElement("tensor-3d");
			writer.WriteAttributeString("d1", t1.Depth.ToString());
			writer.WriteAttributeString("d2", t2.Depth.ToString());
			foreach (var matrix in t1.Matrix.Zip(t2.Matrix, (m1, m2) => (m1, m2)))
				_Write(matrix.Item1, matrix.Item2, writer);
			writer.WriteEndElement();
		}

		static void _Write(IIndexable4DTensor t1, IIndexable4DTensor t2, XmlTextWriter writer)
		{
			writer.WriteStartElement("tensor-4d");
			writer.WriteAttributeString("c1", t1.Count.ToString());
			writer.WriteAttributeString("c2", t2.Count.ToString());
			foreach (var tensor in t1.Tensors.Zip(t2.Tensors, (m1, m2) => (m1, m2)))
				_Write(tensor.Item1, tensor.Item2, writer);
			writer.WriteEndElement();
		}

		static string _WriteComparison(IIndexable3DTensor t1, IIndexable3DTensor t2)
		{
			using (var stringWriter = new StringWriter())
			using (var writer = new XmlTextWriter(stringWriter)) {
				_Write(t1, t2, writer);
				writer.Flush();
				return stringWriter.ToString();
			}
		}

		static string _WriteComparison(IIndexable4DTensor t1, IIndexable4DTensor t2)
		{
			using (var stringWriter = new StringWriter())
			using (var writer = new XmlTextWriter(stringWriter)) {
				_Write(t1, t2, writer);
				writer.Flush();
				return stringWriter.ToString();
			}
		}

		static string _WriteComparison(IIndexableMatrix m1, IIndexableMatrix m2)
		{
			using (var stringWriter = new StringWriter())
			using (var writer = new XmlTextWriter(stringWriter)) {
				_Write(m1, m2, writer);
				writer.Flush();
				return stringWriter.ToString();
			}
		}
	}
}
