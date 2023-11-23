using System;
using BrightWire.Bayesian.Training;
using BrightWire.Helper;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;
using BrightWire.TreeBased.Training;
using BrightWire.Unsupervised;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra;
using BrightData.LinearAlgebra.ReadOnly;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Node;
using BrightWire.InstanceBased.Training;
using BrightWire.Models;
using BrightWire.Models.TreeBased;
using BrightData.Types;

namespace BrightWire
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Random projections allow you to reduce the dimensions of a matrix while still preserving significant information
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="fixedSize">The vector size to reduce from</param>
        /// <param name="reducedSize">The vector size to reduce to</param>
        /// <param name="s"></param>
        public static IRandomProjection CreateRandomProjection(this LinearAlgebraProvider lap, uint fixedSize, uint reducedSize, int s = 3)
        {
            return new RandomProjection(lap, fixedSize, reducedSize, s);
        }

        /// <summary>
        /// Find the next set of state transitions from a pair of observations
        /// </summary>
        /// <typeparam name="T">The type of the model</typeparam>
        /// <param name="model">A markov model saved to a dictionary</param>
        /// <param name="item1">The first observation</param>
        /// <param name="item2">The second observation</param>
        /// <returns>The list of state transitions or null if nothing was found</returns>
        public static MarkovModelStateTransition<T>[]? GetTransitions<T>(this Dictionary<MarkovModelObservation2<T>, MarkovModelStateTransition<T>[]?> model, T item1, T item2) where T: notnull
        {
            var observation = new MarkovModelObservation2<T>(item1, item2);
            if (model.TryGetValue(observation, out var ret))
                return ret;
            return null;
        }

        /// <summary>
        /// Find the next set of state transitions from a tuple of observations
        /// </summary>
        /// <typeparam name="T">The type of the model</typeparam>
        /// <param name="model">A markov model saved to a dictionary</param>
        /// <param name="item1">The first observation</param>
        /// <param name="item2">The second observation</param>
        /// <param name="item3">The third observation</param>
        /// <returns>The list of state transitions or null if nothing was found</returns>
        public static MarkovModelStateTransition<T>[]? GetTransitions<T>(this Dictionary<MarkovModelObservation3<T>, MarkovModelStateTransition<T>[]?> model, T item1, T item2, T item3) where T : notnull
        {
            var observation = new MarkovModelObservation3<T>(item1, item2, item3);
            if (model.TryGetValue(observation, out var ret))
                return ret;
            return null;
        }

        /// <summary>
        /// Non negative matrix factorisation - clustering based on matrix factorisation. Only applicable for training data that is non-negative.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="k">The number of clusters</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <returns>A list of k clusters</returns>
        public static IReadOnlyVector[][] Nnmf(this IEnumerable<IReadOnlyVector> data, LinearAlgebraProvider lap, uint k, uint maxIterations = 1000)
        {
            var clusterer = new NonNegativeMatrixFactorisation(lap, k);
            return clusterer.Cluster(data, maxIterations);
        }

        /// <summary>
        /// Hierarchical clustering successively finds the closest distance between pairs of centroids until k is reached
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <param name="k">The number of clusters to find</param>
        /// <returns>A list of k clusters</returns>
        public static IReadOnlyVector[][] HierarchicalCluster(this IEnumerable<IReadOnlyVector> data, LinearAlgebraProvider lap, uint k)
        {
            using var clusterer = new Hierarchical(lap, k, data);
            clusterer.Cluster();
            return clusterer.Clusters;
        }

        /// <summary>
        /// K Means uses coordinate descent and a distance metric between randomly selected centroids to cluster the data
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <param name="context">Bright data context</param>
        /// <param name="k">The number of clusters to find</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <param name="distanceMetric">Distance metric to use to compare centroids</param>
        /// <returns>A list of k clusters</returns>
        public static IReadOnlyVector[][] KMeans(this IEnumerable<IReadOnlyVector> data, BrightDataContext context, uint k, uint maxIterations = 1000, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            using var kmeans = new KMeans(context, k, data, distanceMetric);
            kmeans.ClusterUntilConverged(maxIterations);
            return kmeans.Clusters;
        }

        /// <summary>
        /// K Nearest Neighbours is an instance based classification method that uses examples from training data to predict classifications
        /// </summary>
        /// <param name="data">The training data</param>
        public static Task<KNearestNeighbours> TrainKNearestNeighbours(this IDataTable data)
        {
            return KnnClassificationTrainer.Train(data);
        }


        /// <summary>
        /// Random forests are built on a bagged collection of features to try to capture the most salient points of the training data without overfitting
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="b">The number of trees in the forest</param>
        /// <param name="baggedRowCount"></param>
        /// <param name="config"></param>
        /// <returns>A model that can be used for classification</returns>
        public static Task<RandomForest> TrainRandomForest(this IDataTable data, uint b = 100, uint? baggedRowCount = null, DecisionTreeTrainer.Config? config = null)
        {
            return RandomForestTrainer.Train(data, b, baggedRowCount, config);
        }

        /// <summary>
        /// Decision trees build a logical tree to classify data. Various measures can be specified to prevent overfitting.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="config"></param>
        /// <returns>A model that can be used for classification</returns>
        public static DecisionTree TrainDecisionTree(this IDataTable data, DecisionTreeTrainer.Config? config = null)
        {
            return DecisionTreeTrainer.Train(data, config);
        }

        /// <summary>
        /// Multinomial naive bayes preserves the count of each feature within the model. Useful for long documents.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <returns>A model that can be used for classification</returns>
        public static MultinomialNaiveBayes TrainMultinomialNaiveBayes(this IEnumerable<IndexListWithLabel<string>> data)
        {
            var trainer = new MultinomialNaiveBayesTrainer();
            foreach (var (classification, indexList) in data)
                trainer.AddClassification(classification, indexList);
            return trainer.Train();
        }

		/// <summary>
		/// Multinomial naive bayes preserves the count of each feature within the model. Useful for long documents.
		/// </summary>
		/// <param name="table">The training data table that must have a index-list based column to classify against</param>
		/// <returns></returns>
	    public static async Task<MultinomialNaiveBayes> TrainMultinomialNaiveBayes(this IDataTable table)
		{
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var indexListColumn = table.ColumnTypes
                .Select((c, i) => (ColumnType: c, Index: (uint)i))
                .Single(c => c.ColumnType == BrightDataType.IndexList);
            if (indexListColumn.Index == targetColumnIndex)
                throw new ArgumentException("No index list column of features");

            var data = await table.MapRows(row => new IndexListWithLabel<string>(row.Get<string>(targetColumnIndex), row.Get<IndexList>(indexListColumn.Index)));
            return data.TrainMultinomialNaiveBayes();
        }

        /// <summary>
        /// Bernoulli naive bayes treats each feature as either 1 or 0 - all feature counts are discarded. Useful for short documents.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <returns>A model that can be used for classification</returns>
        public static BernoulliNaiveBayes TrainBernoulliNaiveBayes(this IEnumerable<IndexListWithLabel<string>> data)
        {
            var trainer = new BernoulliNaiveBayesTrainer();
            foreach (var (classification, indexList) in data)
                trainer.AddClassification(classification, indexList);
            return trainer.Train();
        }

	    /// <summary>
	    /// Bernoulli naive bayes treats each feature as either 1 or 0 - all feature counts are discarded. Useful for short documents.
	    /// </summary>
	    /// <param name="table">The training data table that must have an index-list based column</param>
	    /// <returns>A model that can be used for classification</returns>
	    public static async Task<BernoulliNaiveBayes> TrainBernoulliNaiveBayes(this IDataTable table)
	    {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var indexListColumn = table.ColumnTypes
                .Select((c, i) => (ColumnType: c, Index: (uint)i))
                .Single(c => c.ColumnType == BrightDataType.IndexList);

            var data = await table.MapRows(row => new IndexListWithLabel<string>(row.Get<string>(targetColumnIndex), row.Get<IndexList>(indexListColumn.Index)));
            return data.TrainBernoulliNaiveBayes();
        }

        /// <summary>
        /// Naive bayes is a classifier that assumes conditional independence between all features
        /// </summary>
        /// <param name="table">The training data provider</param>
        /// <returns>A naive bayes model</returns>
        public static Task<NaiveBayes> TrainNaiveBayes(this IDataTable table)
        {
            return NaiveBayesTrainer.Train(table);
        }

        /// <summary>
        /// Finds the classification with the highest weight
        /// </summary>
        /// <param name="classifications">List of weighted classifications</param>
        /// <returns></returns>
        public static string GetBestClassification(this IEnumerable<(string Label, float Weight)> classifications)
        {
            return classifications.MaxBy(c => c.Weight).Label;
        }

        /// <summary>
        /// Trains a neural network with a single hidden layer
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="trainingTable"></param>
        /// <param name="testTable"></param>
        /// <param name="errorMetric"></param>
        /// <param name="learningRate"></param>
        /// <param name="batchSize"></param>
        /// <param name="hiddenLayerSize"></param>
        /// <param name="numIterations"></param>
        /// <param name="activation"></param>
        /// <param name="gradientDescent"></param>
        /// <param name="weightInitialisation"></param>
        /// <returns></returns>
        public static ExecutionGraphModel? TrainSimpleNeuralNetwork(this GraphFactory graph,
            IDataTable trainingTable,
            IDataTable testTable,
            IErrorMetric errorMetric,
            float learningRate,
            uint batchSize,
            uint hiddenLayerSize,
            uint numIterations,
            Func<GraphFactory, NodeBase> activation,
            Func<GraphFactory.GradientDescentProvider, ICreateTemplateBasedGradientDescent> gradientDescent,
            Func<GraphFactory.WeightInitialisationProvider, IWeightInitialisation> weightInitialisation
        )
        {
            // set gradient descent and weight initialisation parameters
            graph.CurrentPropertySet
                .Use(gradientDescent(graph.GradientDescent))
                .Use(weightInitialisation(graph.WeightInitialisation))
            ;

            // create the engine
            var trainingData = graph.CreateDataSource(trainingTable);
            var engine = graph.CreateTrainingEngine(trainingData, errorMetric, learningRate, batchSize);

            // create the network
            graph.Connect(engine)
                // create the initial feed forward layer with activation
                .AddFeedForward(hiddenLayerSize)
                .Add(activation(graph))

                // create a second feed forward layer with activation
                .AddFeedForward(engine.DataSource.GetOutputSizeOrThrow())
                .Add(activation(graph))

                // calculate the error and backpropagate the error signal
                .AddBackpropagation()
            ;

            // train the network, saving the model on each improvement
            ExecutionGraphModel? bestGraph = null;
            var testData = trainingData.CloneWith(testTable);
            engine.Train(numIterations, testData, model => bestGraph = model.Graph);
            engine.Test(testData);
            return bestGraph;
        }
    }
}
