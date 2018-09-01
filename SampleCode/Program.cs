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
using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace BrightWire.SampleCode
{
	partial class Program
	{
		public static void XOR()
		{
			using (var lap = BrightWireProvider.CreateLinearAlgebra()) {
				// Create some training data that the network will learn.  The XOR pattern looks like:
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

					// and xavier weight initialisation
					.Use(graph.WeightInitialisation.Gaussian)
				;

				// create the engine
				var testData = graph.CreateDataSource(data);
				var engine = graph.CreateTrainingEngine(testData, 0.1f, 4);

				// create the network
				const int HIDDEN_LAYER_SIZE = 6;
				graph.Connect(engine)
					// create a feed forward layer with sigmoid activation
					.AddFeedForward(HIDDEN_LAYER_SIZE)
					.Add(graph.SigmoidActivation())

					// create a second feed forward layer with sigmoid activation
					.AddFeedForward(engine.DataSource.OutputSize)
					.Add(graph.SigmoidActivation())

					// backpropagate the error signal at the end of the graph
					.AddBackpropagation(errorMetric)
				;

				// train the network
				var executionContext = graph.CreateExecutionContext();
				for (var i = 0; i < 1000; i++) {
					var trainingError = engine.Train(executionContext);
					if (i % 100 == 0)
						engine.Test(testData, errorMetric);
				}
				engine.Test(testData, errorMetric);

				// create a new network to execute the learned network
				var networkGraph = engine.Graph;
				var executionEngine = graph.CreateEngine(networkGraph);
				var output = executionEngine.Execute(testData);
				Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));

				// print the learnt values
				foreach (var item in output) {
					foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
						var row = data.GetRow(index);
						var result = item.Output[index];
						Console.WriteLine($"{row.GetField<int>(0)} XOR {row.GetField<int>(1)} = {result.Data[0]}");
					}
				}
			}
		}

		public class Datos : IDataSource
		{
			readonly int _inputSize, _outputSize;
			readonly IReadOnlyList<FloatVector> _data;
			readonly ILinearAlgebraProvider _lap;

			public Datos(ILinearAlgebraProvider lap, IReadOnlyList<FloatVector> data)
			{
				_lap = lap;
				_data = data;

				var first = data.First();
				_inputSize = first.Size - 1;
				_outputSize = 1; 
			}

			public int InputCount => 1;
			public bool IsSequential => false;
			public int InputSize => _inputSize;
			public int OutputSize => _outputSize;
			public int RowCount => _data.Count;

			public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
			{
				var data = rows.Select(i => _data[i]).ToList();
				var input = _lap.CreateMatrix(data.Count, InputSize, (x, y) => data[x].Data[y]);
				var output = _lap.CreateMatrix(data.Count, OutputSize, (x, y) => data[x].Data[y + InputSize]);
				var inputList = new List<IGraphData> {input.AsGraphData()};
				return new MiniBatch(rows, this, inputList, output.AsGraphData());
			}

			public IReadOnlyList<IReadOnlyList<int>> GetBuckets()
			{
				return new[] {Enumerable.Range(0, _data.Count).ToList()};
			}

			public IDataSource CloneWith(IDataTable dataTable)
			{
				throw new NotImplementedException();
			}

			public void OnBatchProcessed(IContext context)
			{
				// nop
			}
		}

		static void Main(string[] args)
		{
			// base path to a directory on your computer with training files
			const string DataBasePath = @"c:\data\";

			// base path to a directory on your computer to store model files
			const string ModelBasePath = @"c:\temp\";

			// uncomment to use the (faster) native MKL provider if available
			// Control.UseNativeMKL();

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

			//      var lap = BrightWireProvider.CreateLinearAlgebra();           
			//      GraphFactory graph = new GraphFactory(lap);
			//      var context = graph.CreateExecutionContext(); 

			//      //Indicamos que el algoritmo de backpropagation será el adamoptimizar
			//      graph.CurrentPropertySet
			//	.Use(graph.Adam());

			//      //rate de entrenamiento
			//      const float TRAINING_RATE = 0.5f;

			//      //Creamos los datos (XOR clásico)

			//var lista = new[] {
			//	FloatVector.Create(new[] {0f, 0, 0, 0}),
			//	FloatVector.Create(new[] {0f, 0, 1, 1}),
			//	FloatVector.Create(new[] {0f, 1, 0, 1}),
			//	FloatVector.Create(new[] {0f, 1, 1, 0}),
			//	FloatVector.Create(new[] {1f, 0, 0, 1}),
			//	FloatVector.Create(new[] {1f, 0, 1, 1}),
			//	FloatVector.Create(new[] {1f, 1, 0, 1}),
			//	FloatVector.Create(new[] {1f, 1, 1, 0}),
			//};

			//      Datos d = new Datos(lap, lista);                                                                         

			//      //Cçreamos el motor de entrenamiento, donde se le indican los datos a entrenar el training rate y el tamaño del batch
			//      BrightWire.IGraphTrainingEngine engine = graph.CreateTrainingEngine(d, TRAINING_RATE, 128);            
			//      //esto sirve para ajustar el training rate cada cierto número de batchs, no lo uso ahora mismo.
			//      engine.LearningContext.ScheduleLearningRate(100, TRAINING_RATE / 2);                        

			//      //métrica de error o función de pérdida, cuadrática (mse supongo en el estándar de keras)
			//      var errorMetric = graph.ErrorMetric.Quadratic; 

			//      // create the network
			//      graph.Connect(engine)
			//          .AddFeedForward(outputSize: 4)  //tipo de capa
			//          .Add(graph.ReluActivation()) // tipo de activación.  
			//          .AddFeedForward(outputSize: 1)   //última capa, indicamos el tamaño de salida el del trainingdata
			//          .Add(graph.ReluActivation())
			//          .AddBackpropagation(errorMetric)
			//      ;            

			//      // train the network for twenty iterations, saving the model on each improvement                        
			//      for (int i = 0; i < 1000; i++)
			//      { 
			//          Console.WriteLine (engine.Train(context).ToString()); //la salida de esto parece ser el error total (loss)                                            
			//      }


			//      var resultado = engine.Execute(new float[] { 1f, 0f, 1f });  //probamos a ejecutar a ver cómo nos ha quedado.
			//      Console.WriteLine(resultado.Output[0].ToString());
			//      resultado = engine.Execute(new float[] { 1f, 1f, 1f });
			//	Console.WriteLine(resultado.Output[0].ToString());


			//using (var lap = BrightWireGpuProvider.CreateLinearAlgebra()) {
			//	var rand = new Random();
			//	var list = new List<IVector>();
			//	Console.Write("Loading...");
			//	const int VECTOR_COUNT = 1024, VECTOR_SIZE = 2048;
			//	for (var i = 0; i < VECTOR_COUNT; i++) {
			//		var vector = lap.CreateVector(VECTOR_SIZE, j => (float)rand.NextDouble());
			//		list.Add(vector);
			//	}
			//	Console.WriteLine("done");

			//	var stopwatch = new Stopwatch();
			//	stopwatch.Start();
			//	var clusters = list.KMeans(50);
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

			int depth = 2, filterWidth = 2, filterHeight = 2, filterCount = 3, inputWidth = 4, inputHeight = 4, stride = 2;
			var _cpu = BrightWireProvider.CreateLinearAlgebra();
			using (var _cuda = BrightWireGpuProvider.CreateLinearAlgebra()) {
				using (var cpuTensor = _cpu.Create3DTensor(Enumerable.Range(0, depth).Select(i => _cpu.CreateMatrix(inputWidth, inputHeight, (j, k) => Convert.ToSingle((i + 1) * (j + 1) * (k + 1)))).ToList()))
				using (var gpuTensor = _cuda.Create3DTensor(cpuTensor.AsIndexable().Matrix))
				using (var cpuMatrix = cpuTensor.Im2Col(filterWidth, filterHeight, stride))
				using (var gpuMatrix = gpuTensor.Im2Col(filterWidth, filterHeight, stride)) {
					var im2Col = cpuMatrix.AsIndexable();
					var im2ColGpu = gpuMatrix.AsIndexable();
					var matrix = _WriteMatrixComparison(im2Col, im2ColGpu);

					var normalDistribution = new Normal(0, 1);
					var cpuFilter = _cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (i, j) => (float)normalDistribution.Sample());
					var output = im2Col.Multiply(cpuFilter);

					var matrixList = new List<IMatrix>();
					var newWidth = ((inputWidth - filterWidth) / stride) + 1;
					var newHeight = ((inputHeight - filterHeight) / stride) + 1;
					for (var i = 0; i < output.ColumnCount; i++)
						matrixList.Add(output.Column(i).ConvertInPlaceToMatrix(newWidth, newHeight));
					var outputTensor = _cpu.Create3DTensor(matrixList);

					var cpuFilterList = new List<IReadOnlyList<IVector>>();
					for (var i = 0; i < cpuFilter.ColumnCount; i++)
						cpuFilterList.Add(cpuFilter.Column(i).Split(depth).Select(v => v.Rotate(v.Count / filterWidth / filterHeight)).ToList());

					using (var gpuOutputTensor = _cuda.Create3DTensor(outputTensor.AsIndexable())) {
						var gpuFilterList = cpuFilterList.Select(fl => fl.Select(f => _cuda.CreateVector(f.AsIndexable())).ToList()).ToList();

						var reverseIm2Col = outputTensor.ReverseIm2Col(cpuFilterList, inputHeight, inputWidth, 0, filterWidth, filterHeight, stride);
						using (var gpuReverseIm2Col = gpuOutputTensor.ReverseIm2Col(gpuFilterList, inputHeight, inputWidth, 0, filterWidth, filterHeight, stride)) {
							var comparison = _WriteMatrixComparison(reverseIm2Col.AsIndexable(), gpuReverseIm2Col.AsIndexable());
						}
					}
				}
			}
			//var matrix = _TensorReverseIm2Col(2, 2, 2, 2, 1, 4, 4);
		}

		static string _WriteMatrixComparison(IIndexableMatrix m1, IIndexableMatrix m2)
		{
			using(var stringWriter = new StringWriter())
			using (var writer = new XmlTextWriter(stringWriter)) {
				writer.WriteStartElement("matrix");

				foreach (var row in m1.Rows.Zip(m2.Rows, (x, y) => (x, y))) {
					writer.WriteStartElement("row");
					foreach (var cell in row.Item1.Values.Zip(row.Item2.Values, (i, j) => (i, j))) {
						writer.WriteStartElement("cell");
						writer.WriteAttributeString("v1", cell.Item1.ToString(CultureInfo.InvariantCulture));
						writer.WriteAttributeString("v2", cell.Item2.ToString(CultureInfo.InvariantCulture));
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.Flush();
				return stringWriter.ToString();
			}
		}

		static string _TensorReverseIm2Col(int filterWidth, int filterHeight, int stride, int depth, int filterCount, int inputWidth, int inputHeight)
		{
			using (var cpu = BrightWireProvider.CreateLinearAlgebra())
			using (var gpu = BrightWireGpuProvider.CreateLinearAlgebra()) {
				var normalDistribution = new Normal(0, 1);
				var cpuTensor = cpu.Create3DTensor(Enumerable.Range(0, depth).Select(i => cpu.CreateMatrix(inputHeight, inputWidth, (j, k) => Convert.ToSingle(normalDistribution.Sample()))).ToList());
				var im2Col = cpuTensor.Im2Col(filterWidth, filterHeight, stride);
				var cpuFilter = cpu.CreateMatrix(depth * filterWidth * filterHeight, filterCount, (i, j) => (float) normalDistribution.Sample());
				var output = im2Col.Multiply(cpuFilter);

				var matrixList = new List<IMatrix>();
				var newWidth = ((inputWidth - filterWidth) / stride) + 1;
				var newHeight = ((inputHeight - filterHeight) / stride) + 1;
				for (var i = 0; i < output.ColumnCount; i++)
					matrixList.Add(output.Column(i).ConvertInPlaceToMatrix(newWidth, newHeight));
				var outputTensor = cpu.Create3DTensor(matrixList);

				var cpuFilterList = new List<IReadOnlyList<IVector>>();
				for (var i = 0; i < cpuFilter.ColumnCount; i++)
					cpuFilterList.Add(cpuFilter.Column(i).Split(depth).Select(v => v.Rotate()).ToList());

				using (var gpuTensor = gpu.Create3DTensor(outputTensor.AsIndexable())) {
					var gpuFilterList = cpuFilterList.Select(fl => fl.Select(f => gpu.CreateVector(f.AsIndexable())).ToList()).ToList();

					var cpuReverseIm2Col = outputTensor.ReverseIm2Col(cpuFilterList, inputHeight, inputWidth, 0, filterWidth, filterHeight, stride);
					using (var gpuReverseIm2Col = gpuTensor.ReverseIm2Col(gpuFilterList, inputHeight, inputWidth, 0, filterWidth, filterHeight, stride)) {
						return _WriteMatrixComparison(cpuReverseIm2Col.AsIndexable(), gpuReverseIm2Col.AsIndexable());
					}
				}
			}
		}
	}
}
