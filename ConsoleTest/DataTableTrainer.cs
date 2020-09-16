using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData.Numerics;
using BrightTable;
using BrightWire;
using BrightWire.ExecutionGraph;
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

        public ExecutionGraph TrainSigmoidNeuralNetwork(uint hiddenLayerSize, float learningRate, uint batchSize, bool writeResults = true)
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

				// create a second feed forward layer with sigmoid activation
				.AddFeedForward(engine.DataSource.OutputSize ?? throw new Exception("No output"))
				.Add(graph.SigmoidActivation())

				// calculate the error and backpropagate the error signal
				.AddBackpropagation(errorMetric)
			;

			// train the network
			var executionContext = graph.CreateExecutionContext();
            var testData = graph.CreateDataSource(Test);
			for (var i = 0; i < 1000; i++) {
				engine.Train(executionContext);
				if (i % 100 == 0)
					engine.Test(testData, errorMetric);
			}
			engine.Test(testData, errorMetric);

			// create a new network to execute the learned network
			var networkGraph = engine.Graph;
			var executionEngine = graph.CreateEngine(networkGraph);
			var output = executionEngine.Execute(testData).ToList();
			Console.WriteLine(output.Average(o => o.CalculateError(errorMetric)));

			// print the values that have been learned
			foreach (var item in output) {
				foreach (var index in item.MiniBatchSequence.MiniBatch.Rows) {
					var row = Table.Row(index);
					var result = item.Output[index];
					Console.WriteLine($"{row} = {result}");
				}
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
