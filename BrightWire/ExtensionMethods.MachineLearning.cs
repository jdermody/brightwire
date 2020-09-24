using System;
using BrightWire.Bayesian.Training;
using BrightWire.Helper;
using BrightWire.Linear.Training;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;
using BrightWire.TreeBased.Training;
using BrightWire.Unsupervised;
using System.Collections.Generic;
using System.Linq;
using BrightTable;
using BrightData;
using BrightWire.InstanceBased.Training;
using BrightWire.Models.Linear;
using BrightWire.Models.TreeBased;

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
        public static IRandomProjection CreateRandomProjection(this ILinearAlgebraProvider lap, uint fixedSize, uint reducedSize, int s = 3)
        {
            return new RandomProjection(lap, fixedSize, reducedSize, s);
        }

	    /// <summary>
	    /// Trains a logistic regression model on a data table
	    /// </summary>
	    /// <param name="table">The training data</param>
        /// <param name="iterations">Number of iterations to train for</param>
	    /// <param name="learningRate">The learning rate</param>
	    /// <param name="lambda">Regularisation lambda</param>
	    /// <param name="costCallback">Optional callback that is called after each iteration with the current cost</param>
	    /// <returns>The trained model</returns>
	    public static LogisticRegression TrainLogisticRegression(this IRowOrientedDataTable table, uint iterations, float learningRate, float lambda = 0.1f, Func<float, bool> costCallback = null)
        {
            var trainer = table.CreateLogisticRegressionTrainer();
            return trainer.GradientDescent(iterations, learningRate, lambda, costCallback);
        }

        /// <summary>
        /// Logistic regression learns a sigmoid function over a set of data that learns to classify future values into positive or negative samples
        /// </summary>
        /// <param name="table">The training data provider</param>
        /// <returns>A trainer that can be used to build a logistic regression model</returns>
        public static ILogisticRegressionTrainer CreateLogisticRegressionTrainer(this IRowOrientedDataTable table)
        {
            return new LogisticRegressionTrainer(table);
        }

        /// <summary>
        /// Find the next set of state transitions from a pair of observations
        /// </summary>
        /// <typeparam name="T">The type of the model</typeparam>
        /// <param name="model">A markov model saved to a dictionary</param>
        /// <param name="item1">The first observation</param>
        /// <param name="item2">The second observation</param>
        /// <returns>The list of state transitions or null if nothing was found</returns>
        public static MarkovModelStateTransition<T>[] GetTransitions<T>(this Dictionary<MarkovModelObservation2<T>, MarkovModelStateTransition<T>[]> model, T item1, T item2)
        {
            var observation = new MarkovModelObservation2<T> {
                Item1 = item1,
                Item2 = item2
            };
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
        public static MarkovModelStateTransition<T>[] GetTransitions<T>(this Dictionary<MarkovModelObservation3<T>, MarkovModelStateTransition<T>[]> model, T item1, T item2, T item3)
        {
            var observation = new MarkovModelObservation3<T> {
                Item1 = item1,
                Item2 = item2,
                Item3 = item3
            };
            if (model.TryGetValue(observation, out var ret))
                return ret;
            return null;
        }

        /// <summary>
        /// Non negative matrix factorisation - clustering based on matrix factorisation. Only applicable for training data that is non-negative.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="lap">Linear alegbra provider</param>
        /// <param name="k">The number of clusters</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <returns>A list of k clusters</returns>
        public static IFloatVector[][] NNMF(this IEnumerable<IFloatVector> data, ILinearAlgebraProvider lap, int k, int maxIterations = 1000)
        {
            var clusterer = new NonNegativeMatrixFactorisation(lap, k);
            return clusterer.Cluster(data, maxIterations);
        }

        /// <summary>
        /// Hierachical clustering successively finds the closest distance between pairs of centroids until k is reached
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <param name="k">The number of clusters to find</param>
        /// <returns>A list of k clusters</returns>
        public static IFloatVector[][] HierachicalCluster(this IEnumerable<IFloatVector> data, int k)
        {
            using var clusterer = new Hierachical(k, data);
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
        public static IFloatVector[][] KMeans(this IEnumerable<IFloatVector> data, IBrightDataContext context, int k, int maxIterations = 1000, DistanceMetric distanceMetric = DistanceMetric.Euclidean)
        {
            using var clusterer = new KMeans(context, k, data, distanceMetric);
            clusterer.ClusterUntilConverged(maxIterations);
            return clusterer.Clusters;
        }

        /// <summary>
        /// K Nearest Neighbours is an instance based classification method that uses examples from training data to predict classifications
        /// </summary>
        /// <param name="data">The training data</param>
        public static KNearestNeighbours TrainKNearestNeighbours(this IDataTable data)
        {
            return KNNClassificationTrainer.Train(data);
        }

	    /// <summary>
	    /// Multinomial Logistic Regression generalises Logistic Regression to multi-class classification
	    /// </summary>
	    /// <param name="data">The training data</param>
        /// <param name="trainingIterations">Number of training iterations</param>
	    /// <param name="trainingRate">Training rate</param>
	    /// <param name="lambda">L2 regularisation</param>
	    /// <param name="costCallback">Optional callback that is called after each iteration with the current cost</param>
	    /// <returns></returns>
	    public static MultinomialLogisticRegression TrainMultinomialLogisticRegression(this IRowOrientedDataTable data, uint trainingIterations, float trainingRate, float lambda = 0.1f, Func<float, bool> costCallback = null)
        {
            return MultinomialLogisticRegressionTrainner.Train(data, trainingIterations, trainingRate, lambda, costCallback);
        }

        /// <summary>
        /// Random forests are built on a bagged collection of features to try to capture the most salient points of the training data without overfitting
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="b">The number of trees in the forest</param>
        /// <param name="baggedRowCount"></param>
        /// <param name="config"></param>
        /// <returns>A model that can be used for classification</returns>
        public static RandomForest TrainRandomForest(this IRowOrientedDataTable data, uint b = 100, uint? baggedRowCount = null, DecisionTreeTrainer.Config config = null)
        {
            return RandomForestTrainer.Train(data, b, baggedRowCount, config);
        }

        /// <summary>
        /// Decision trees build a logical tree to classify data. Various measures can be specified to prevent overfitting.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="config"></param>
        /// <returns>A model that can be used for classification</returns>
        public static DecisionTree TrainDecisionTree(this IRowOrientedDataTable data, DecisionTreeTrainer.Config config = null)
        {
            return DecisionTreeTrainer.Train(data, config);
        }

        /// <summary>
        /// Multinomial naive bayes preserves the count of each feature within the model. Useful for long documents.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <returns>A model that can be used for classification</returns>
        public static MultinomialNaiveBayes TrainMultinomialNaiveBayes(this IEnumerable<(string Classification, IndexList Data)> data)
        {
            var trainer = new MultinomialNaiveBayesTrainer();
            foreach (var item in data)
                trainer.AddClassification(item.Classification, item.Data);
            return trainer.Train();
        }

		/// <summary>
		/// Multinomial naive bayes preserves the count of each feature within the model. Useful for long documents.
		/// </summary>
		/// <param name="table">The training data table that must have a index-list based column to classify against</param>
		/// <returns></returns>
	    public static MultinomialNaiveBayes TrainMultinomialNaiveBayes(this IRowOrientedDataTable table)
		{
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var indexListColumn = table.ColumnTypes
                .Select((c, i) => (ColumnType: c, Index: (uint)i))
                .Single(c => c.ColumnType == ColumnType.IndexList);
            if (indexListColumn.Index == targetColumnIndex)
                throw new ArgumentException("No index list column of features");

            var data = table.AsConvertible()
                .Map((row => (row.GetField<string>(targetColumnIndex), row.GetField<IndexList>(indexListColumn.Index))));
            return data.TrainMultinomialNaiveBayes();
        }

        /// <summary>
        /// Bernoulli naive bayes treats each feature as either 1 or 0 - all feature counts are discarded. Useful for short documents.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <returns>A model that can be used for classification</returns>
        public static BernoulliNaiveBayes TrainBernoulliNaiveBayes(this IEnumerable<(string Classification, IndexList Data)> data)
        {
            var trainer = new BernoulliNaiveBayesTrainer();
            foreach (var item in data)
                trainer.AddClassification(item.Classification, item.Data);
            return trainer.Train();
        }

	    /// <summary>
	    /// Bernoulli naive bayes treats each feature as either 1 or 0 - all feature counts are discarded. Useful for short documents.
	    /// </summary>
	    /// <param name="table">The training data table that must have an index-list based column</param>
	    /// <returns>A model that can be used for classification</returns>
	    public static BernoulliNaiveBayes TrainBernoulliNaiveBayes(this IRowOrientedDataTable table)
	    {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var indexListColumn = table.ColumnTypes
                .Select((c, i) => (ColumnType: c, Index: (uint)i))
                .Single(c => c.ColumnType == ColumnType.IndexList);

            var data = table.AsConvertible()
                .Map(row => (row.GetField<string>(targetColumnIndex), row.GetField<IndexList>(indexListColumn.Index)));
            return data.TrainBernoulliNaiveBayes();
        }

        /// <summary>
        /// Naive bayes is a classifier that assumes conditional independence between all features
        /// </summary>
        /// <param name="table">The training data provider</param>
        /// <returns>A naive bayes model</returns>
        public static NaiveBayes TrainNaiveBayes(this IDataTable table)
        {
            return NaiveBayesTrainer.Train(table);
        }

        /// <summary>
        /// Linear regression fits a line to a set of data that allows you predict future values
        /// </summary>
        /// <param name="table">The training data table</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns>A trainer that can be used to build a linear regression model</returns>
        public static ILinearRegressionTrainer CreateLinearRegressionTrainer(this IDataTable table, ILinearAlgebraProvider lap)
        {
            return new RegressionTrainer(lap, table);
        }

        /// <summary>
        /// Finds the classification with the highest weight
        /// </summary>
        /// <param name="classifications">List of weighted classifications</param>
        /// <returns></returns>
        public static string GetBestClassification(this IEnumerable<(string Label, float Weight)> classifications)
        {
            return classifications.OrderByDescending(c => c.Weight).First().Label;
        }
    }
}
