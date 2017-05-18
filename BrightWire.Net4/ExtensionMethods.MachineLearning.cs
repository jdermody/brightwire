using BrightWire.Bayesian.Training;
using BrightWire.Helper;
using BrightWire.InstanceBased.Trainer;
using BrightWire.Linear.Training;
using BrightWire.Models;
using BrightWire.Models.Bayesian;
using BrightWire.Models.InstanceBased;
using BrightWire.TreeBased.Training;
using BrightWire.Unsupervised;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static IRandomProjection CreateRandomProjection(this ILinearAlgebraProvider lap, int fixedSize, int reducedSize, int s = 3)
        {
            return new RandomProjection(lap, fixedSize, reducedSize, s);
        }

        /// <summary>
        /// Trains a logistic regression model on a data table
        /// </summary>
        /// <param name="table">The training data</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="iterations">Number of iterations to train for</param>
        /// <param name="learningRate">The learning rate</param>
        /// <param name="lambda">Regularisation lambda</param>
        /// <returns>The trained model</returns>
        public static LogisticRegression TrainLogisticRegression(this IDataTable table, ILinearAlgebraProvider lap, int iterations, float learningRate, float lambda = 0.1f)
        {
            var trainer = table.CreateLogisticRegressionTrainer(lap);
            return trainer.GradientDescent(iterations, learningRate, lambda);
        }

        /// <summary>
        /// Logistic regression learns a sigmoid function over a set of data that learns to classify future values into positive or negative samples
        /// </summary>
        /// <param name="table">The training data provider</param>
        /// <param name="lap">Linear algebra provider</param>
        /// <returns>A trainer that can be used to build a logistic regression model</returns>
        public static ILogisticRegressionTrainer CreateLogisticRegressionTrainer(this IDataTable table, ILinearAlgebraProvider lap)
        {
            return new LogisticRegressionTrainer(lap, table);
        }

        /// <summary>
        /// Find the next set of state transitions from a pair of observations
        /// </summary>
        /// <typeparam name="T">The type of the model</typeparam>
        /// <param name="model">A markov model saved to a dictionary</param>
        /// <param name="item1">The first observation</param>
        /// <param name="item2">The second observation</param>
        /// <returns>The list of state transitions or null if nothing was found</returns>
        public static List<MarkovModelStateTransition<T>> GetTransitions<T>(this Dictionary<MarkovModelObservation2<T>, List<MarkovModelStateTransition<T>>> model, T item1, T item2)
        {
            var observation = new MarkovModelObservation2<T> {
                Item1 = item1,
                Item2 = item2
            };
            List<MarkovModelStateTransition<T>> ret;
            if (model.TryGetValue(observation, out ret))
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
        public static List<MarkovModelStateTransition<T>> GetTransitions<T>(this Dictionary<MarkovModelObservation3<T>, List<MarkovModelStateTransition<T>>> model, T item1, T item2, T item3)
        {
            var observation = new MarkovModelObservation3<T> {
                Item1 = item1,
                Item2 = item2,
                Item3 = item3
            };
            List<MarkovModelStateTransition<T>> ret;
            if (model.TryGetValue(observation, out ret))
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
        public static IReadOnlyList<IReadOnlyList<IVector>> NNMF(this IReadOnlyList<IVector> data, ILinearAlgebraProvider lap, int k, int maxIterations = 1000)
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
        public static IReadOnlyList<IReadOnlyList<IVector>> HierachicalCluster(this IReadOnlyList<IVector> data, int k)
        {
            using (var clusterer = new Hierachical(k, data, DistanceMetric.Euclidean)) {
                clusterer.Cluster();
                return clusterer.Clusters;
            }
        }

        /// <summary>
        /// K Means uses coordinate descent and the euclidean distance between randomly selected centroids to cluster the data
        /// </summary>
        /// <param name="data">The list of vectors to cluster</param>
        /// <param name="k">The number of clusters to find</param>
        /// <param name="maxIterations">The maximum number of iterations</param>
        /// <returns>A list of k clusters</returns>
        public static IReadOnlyList<IReadOnlyList<IVector>> KMeans(this IReadOnlyList<IVector> data, int k, int maxIterations = 1000)
        {
            using (var clusterer = new KMeans(k, data, DistanceMetric.Euclidean)) {
                clusterer.ClusterUntilConverged(maxIterations);
                return clusterer.Clusters;
            }
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
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="trainingIterations">Number of training iterations</param>
        /// <param name="trainingRate">Training rate</param>
        /// <param name="lambda">L2 regularisation</param>
        /// <returns></returns>
        public static MultinomialLogisticRegression TrainMultinomialLogisticRegression(this IDataTable data, ILinearAlgebraProvider lap, int trainingIterations, float trainingRate, float lambda = 0.1f)
        {
            return MultinomialLogisticRegressionTrainner.Train(data, lap, trainingIterations, trainingRate, lambda);
        }

        /// <summary>
        /// Random forests are built on a bagged collection of features to try to capture the most salient points of the training data without overfitting
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="b">The number of trees in the forest</param>
        /// <returns>A model that can be used for classification</returns>
        public static RandomForest TrainRandomForest(this IDataTable data, int b = 100)
        {
            return RandomForestTrainer.Train(data, b);
        }

        /// <summary>
        /// Decision trees build a logical tree to classify data. Various measures can be specified to prevent overfitting.
        /// </summary>
        /// <param name="data">The training data</param>
        /// <param name="minDataPerNode">Minimum number of data points per node to continue splitting</param>
        /// <param name="maxDepth">The maximum depth of each leaf</param>
        /// <param name="minInformationGain">The minimum information gain to continue splitting</param>
        /// <param name="maxAttributes">The maximum number of attributes to consider at each split</param>
        /// <returns>A model that can be used for classification</returns>
        public static DecisionTree TrainDecisionTree(this IDataTable data, int? minDataPerNode = null, int? maxDepth = null, double? minInformationGain = null, int? maxAttributes = null)
        {
            var config = new DecisionTreeTrainer.Config {
                MinDataPerNode = minDataPerNode,
                MaxDepth = maxDepth,
                MinInformationGain = minInformationGain,
                MaxAttributes = maxAttributes
            };
            return DecisionTreeTrainer.Train(data, config);
        }

        static void _IterateIndexList(IDataTable dataTable, int inputColumnIndex, Action<IndexList, string> callback)
        {
            var inputColumn = dataTable.Columns[inputColumnIndex];
            var targetColumnIndex = dataTable.TargetColumnIndex;
            var outputColumn = dataTable.Columns[targetColumnIndex];
            if (inputColumn?.Type != ColumnType.IndexList)
                throw new ArgumentException("Input column is not an IndexList");
            if (outputColumn == null || outputColumn != inputColumn)
                throw new ArgumentException("Output column is invalid");

            dataTable.ForEach(row => {
                var input = row.GetField<IndexList>(inputColumnIndex);
                var output = row.GetField<string>(targetColumnIndex);
                callback(input, output);
            });
        }

        /// <summary>
        /// Multinomial naive bayes preserves the count of each feature within the model. Useful for long documents.
        /// </summary>
        /// <param name="dataTable">The training data table</param>
        /// <param name="inputColumnIndex">The column index of the IndexList to classify</param>
        /// <returns>A model that can be used for classification</returns>
        public static MultinomialNaiveBayes TrainMultinomialNaiveBayes(this IDataTable dataTable, int inputColumnIndex = 0)
        {
            var trainer = new MultinomialNaiveBayesTrainer();
            _IterateIndexList(dataTable, inputColumnIndex, (input, output) => trainer.AddClassification(output, input.Index));
            return trainer.Train();
        }

        /// <summary>
        /// Bernoulli naive bayes treats each feature as either 1 or 0 - all feature counts are discarded. Useful for short documents.
        /// </summary>
        /// <param name="dataTable">The training data table</param>
        /// <param name="inputColumnIndex">The column index of the IndexList to classify</param>
        /// <returns>A model that can be used for classification</returns>
        public static BernoulliNaiveBayes TrainBernoulliNaiveBayes(this IDataTable dataTable, int inputColumnIndex = 0)
        {
            var trainer = new BernoulliNaiveBayesTrainer();
            _IterateIndexList(dataTable, inputColumnIndex, (input, output) => trainer.AddClassification(output, input.Index));
            return trainer.Train();
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

        public static string GetBestClassification(this IReadOnlyList<(string Label, float Weight)> classifications)
        {
            return classifications.OrderByDescending(c => c.Weight).First().Label;
        }
    }
}
