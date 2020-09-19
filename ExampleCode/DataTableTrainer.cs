using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Numerics;
using BrightTable;
using BrightTable.Transformations;
using BrightWire;
using BrightWire.ExecutionGraph;
using BrightWire.Linear;
using BrightWire.Linear.Training;
using BrightWire.Models;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;

namespace ExampleCode
{
    class DataTableTrainer
    {
        public DataTableTrainer(IRowOrientedDataTable table)
        {
            TargetColumn = table.GetTargetColumnOrThrow();
            Table = table;
            var split = table.Split();
            Training = split.Training;
            Test = split.Test;
        }

        public DataTableTrainer(IRowOrientedDataTable table, IRowOrientedDataTable training, IRowOrientedDataTable test)
        {
            TargetColumn = table.GetTargetColumnOrThrow();
            Table = table;
            Training = training;
            Test = test;
		}

        public uint TargetColumn { get; }
        public IRowOrientedDataTable Table { get; }
        public IRowOrientedDataTable Training { get; }
        public IRowOrientedDataTable Test { get; }

        public IEnumerable<string> KMeans(int k) => AggregateLabels(Table.KMeans(k));
        public IEnumerable<string> HierachicalCluster(int k) => AggregateLabels(Table.HierachicalCluster(k));
        public IEnumerable<string> NonNegativeMatrixFactorisation(int k) => AggregateLabels(Table.NonNegativeMatrixFactorisation(k));

        IEnumerable<string> AggregateLabels(IEnumerable<(uint RowIndex, string Label)[]> clusters) => clusters
            .Select(c => String.Join(';', c
                .Select(r => r.Label)
                .GroupBy(d => d)
                .Select(g => (Label: g.Key, Count: g.Count()))
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Label)
                .Select(g => $"{g.Label} ({g.Count})")
            ));

        public NaiveBayes TrainNaiveBayes(bool writeResults = true)
        {
            var ret = Training.TrainNaiveBayes();
            if (writeResults)
                WriteResults("Naive bayes", ret.CreateClassifier());
            return ret;
        }

        public DecisionTree TrainDecisionTree(bool writeResults = true)
        {
            var ret = Training.TrainDecisionTree();
            if (writeResults)
                WriteResults("Decision tree", ret.CreateClassifier());
            return ret;
        }

        public RandomForest TrainRandomForest(uint numTrees, uint? baggedRowCount = null, bool writeResults = true)
        {
            var ret = Training.TrainRandomForest(numTrees, baggedRowCount);
            if(writeResults)
                WriteResults("Random forest", ret.CreateClassifier());
            return ret;
        }

        public KNearestNeighbours TrainKNearestNeighbours(uint k, bool writeResults = true)
        {
            var ret = Training.TrainKNearestNeighbours();
            if (writeResults)
                WriteResults("K nearest neighbours", ret.CreateClassifier(Table.Context.LinearAlgebraProvider, k));
            return ret;
        }

        public void TrainMultinomialLogisticRegression(uint iterations, float trainingRate, bool writeResults = true)
        {
            var ret = Training.TrainMultinomialLogisticRegression(iterations, trainingRate);
            if (writeResults)
                WriteResults("Multinomial logistic regression", ret.CreateClassifier(Table.Context.LinearAlgebraProvider));
        }

        public MultinomialLogisticRegression TrainLegacyMultinomialLogisticRegression(uint iterations, float trainingRate, float lambda, bool writeResults = true)
        {
            var lap = Table.Context.LinearAlgebraProvider;
            var ret = LegacyMultinomialLogisticRegressionTrainner.Train(Training, lap, iterations, trainingRate, lambda);
            if (writeResults) {
                var classifier = new LegacyMultinomialLogisticRegressionClassifier(lap, ret);
                WriteResults("(Legacy) Multinomial logistic regression", classifier);
            }

            return ret;
        }

        public void TrainSigmoidNeuralNetwork(uint hiddenLayerSize, uint numIterations, float trainingRate, uint batchSize)
        {
            // create a neural network graph factory
            var graph = new GraphFactory(Table.Context.LinearAlgebraProvider);

            // the default data table -> vector conversion uses one hot encoding of the classification labels, so create a corresponding cost function
            var errorMetric = graph.ErrorMetric.OneHotEncoding;

            // create the property set (use rmsprop gradient descent optimisation)
            graph.CurrentPropertySet
                .Use(graph.RmsProp())
                .Use(graph.GaussianWeightInitialisation(true, 0.1f, GaussianVarianceCalibration.SquareRoot2N));

            // create the training and test data sources
            var trainingData = graph.CreateDataSource(Training);
            var testData = trainingData.CloneWith(Test);

            // create a 4x8x3 neural network with sigmoid activations after each neural network
            var engine = graph.CreateTrainingEngine(trainingData, trainingRate, batchSize, TrainingErrorCalculation.TrainingData);
            graph.Connect(engine)
                .AddFeedForward(hiddenLayerSize)
                .Add(graph.SigmoidActivation())
                .AddDropOut(dropOutPercentage: 0.5f)
                .AddFeedForward(engine.DataSource.OutputSize ?? 0)
                .Add(graph.SigmoidActivation())
                .AddBackpropagation(errorMetric);

            // train the network
            Console.WriteLine("Training a 4x8x3 neural network...");
            engine.Train(numIterations, testData, errorMetric, null, 50);
        }

        public ExecutionGraph TrainSimpleSigmoidNeuralNetwork(uint hiddenLayerSize, uint numIterations, float learningRate, uint batchSize, bool writeResults = true)
        {
            var context = Table.Context;

			// use numerics (cpu based linear algebra)
			var lap = context.UseNumericsLinearAlgebra();

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
                //foreach (var item in output) {
                //    foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
                //        var row = Test.Row(index);
                //        var result = item.Output[index];
                //        Console.WriteLine($"{row} = {result}");
                //    }
                //}
            }

            return networkGraph;
        }

        void WriteResults(string type, IRowClassifier classifier)
        {
            var results = Test.Classify(classifier).ToList();
            var score = results
                .Average(d => d.Row.GetField<string>(TargetColumn) == d.Classification.OrderByDescending(d => d.Weight).First().Label ? 1.0 : 0.0);

            Console.WriteLine($"{type} accuracy: {score:P}");
        }

        void WriteResults(string type, ITableClassifier classifier)
        {
            var results = classifier.Classify(Test).ToList();
            var expectedLabels = Test.Column(TargetColumn).Enumerate().Select(o => o.ToString()).ToArray();
            var score = results
                .Average(d => expectedLabels[d.RowIndex] == d.Predictions.OrderByDescending(d => d.Weight).First().Classification ? 1.0 : 0.0);

            Console.WriteLine($"{type} accuracy: {score:P}");
        }
    }
}
