using BrightWire.ExecutionGraph;
using BrightWire.TrainingData.Artificial;
using System;
using System.IO;
using System.Linq;
using MathNet.Numerics;

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

		public static void BatchNorm()
        {
            // download the iris data set
	        byte[] data = File.ReadAllBytes(@"C:\data\iris.data");

            // parse the iris CSV into a data table
            var dataTable = new StreamReader(new MemoryStream(data)).ParseCSV(',')/*.Normalise(NormalisationType.Standard)*/;
	        var analysis = dataTable.GetAnalysis();

            // the last column is the classification target ("Iris-setosa", "Iris-versicolor", or "Iris-virginica")
            var targetColumnIndex = dataTable.TargetColumnIndex = dataTable.ColumnCount - 1;

            // split the data table into training and test tables
            var split = dataTable.Split(0);

            // fire up some linear algebra on the CPU
            using (var lap = BrightWireGpuProvider.CreateLinearAlgebra(false)) {
                var graph = new GraphFactory(lap);

                // the default data table -> vector conversion uses one hot encoding of the classification labels, so create a corresponding cost function
                var errorMetric = graph.ErrorMetric.OneHotEncoding;

                // create the property set (use rmsprop gradient descent optimisation)
                graph.CurrentPropertySet
                    .Use(graph.RmsProp())
                ;

                // create the training and test data sources
                var trainingData = graph.CreateDataSource(split.Training);
                var testData = trainingData.CloneWith(split.Test);

                // create a 4x3x3 neural network with sigmoid activations after each neural network
                const int HIDDEN_LAYER_SIZE = 16, BATCH_SIZE = 120;
                const float LEARNING_RATE = 0.1f;
                var engine = graph.CreateTrainingEngine(trainingData, LEARNING_RATE, BATCH_SIZE, TrainingErrorCalculation.Fast);
                graph.Connect(engine)
	                .AddBatchNormalisation()
                    .AddFeedForward(HIDDEN_LAYER_SIZE)
	                .AddBatchNormalisation()
                    .Add(graph.SigmoidActivation())
	                //.AddDropOut(0.5f)
                    .AddFeedForward(engine.DataSource.OutputSize)
                    .Add(graph.SoftMaxActivation())
                    .AddBackpropagation(errorMetric)
                ;

                // train the network
                Console.WriteLine("Training neural network...");
                engine.Train(5000, testData, errorMetric, null, 50);
            }
        }

		static void Main(string[] args)
		{
			// base path to a directory on your computer with training files
			const string DataBasePath = @"c:\data\";

			// base path to a directory on your computer to store model files
			const string ModelBasePath = @"c:\temp\";

			// use the (faster) native MKL provider if available
			//Control.UseNativeMKL();

			//BatchNorm();

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
			TrainWithSelu(DataBasePath + @"iris.data");
			//SimpleLinearTest();
			//PredictBicyclesWithLinearModel(DataBasePath + @"bikesharing\hour.csv");
			//PredictBicyclesWithNeuralNetwork(DataBasePath + @"bikesharing\hour.csv");
			//MultiLabelSingleClassifier(DataBasePath + @"emotions\emotions.arff");
			//MultiLabelMultiClassifiers(DataBasePath + @"emotions\emotions.arff");
			//StockData(DataBasePath + @"plotly\stockdata.csv");
		}
	}
}
